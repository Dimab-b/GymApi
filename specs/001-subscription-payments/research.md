# Phase 0 Research: Subscription Payments

## Decision: Plans come from configuration, not a database table

**Rationale**: The gym sells a small number of predefined plans. The project
already has `SubscriptionPlans` in `appsettings` and `ISubscriptionPlanProvider`.
Configuration avoids admin CRUD, migrations, and a second source of truth for a
feature that only needs read access.

**Alternatives considered**:
- A `SubscriptionPlan` database aggregate with admin CRUD. Rejected for now: more
  scope and runtime editing is not required; can be added later without changing
  the purchase contract because periods reference a plan code.

## Decision: `Payment` is a separate aggregate root

**Rationale**: Payments have their own lifecycle (status, future refunds), grow
independently of members, and are a finance concern. A separate root keeps
`Member` small and makes revenue queries natural.

**Alternatives considered**:
- `Payment` as a child of `Member`: simpler atomicity but produces a god aggregate
  and a poor finance boundary. Rejected.

## Decision: One transaction for subscription period and payment

**Rationale**: The spec requires the period and its payment to be recorded
atomically (FR-009). In a monolith, saving both aggregates in one `SaveChanges`
gives that guarantee without a saga. Constitution IV explicitly allows it for
purchases.

**Alternatives considered**:
- Saga/process manager with eventual consistency: correct for microservices but
  introduces compensation and failure windows before the project is split.
  Deferred; the `Payment.Status` field keeps the model ready for it.

## Decision: Renewal may choose any available plan and creates a new period

**Rationale**: The spec clarification accepts any plan. Each purchase or renewal
is a separate record with the plan used and the total price paid, so there is no
snapshot of a previous plan and no mutation of an existing period.

**Alternatives considered**:
- Renewal must use the current plan and locked price: simpler but prevents plan
  changes and contradicts the clarification. Rejected.
- Mutating one long subscription: rejected earlier because history and pricing
  become mixed.

## Decision: Prepayment capped at 12 months from the first current/future period

**Rationale**: Members may prepay multiple periods, but the spec clarification
caps total coverage at 12 months measured from the start of the earliest current
or future period. The domain method computes the next period start and validates
the resulting coverage before adding a period.

**Alternatives considered**:
- No cap: allows unbounded sales far into the future and complicates refunds.
  Rejected.
- Cap from today: inconsistent when an active period started months ago.
  Rejected.
- Cap from the member's first-ever subscription: blocks all future purchases once
  a year has passed. Rejected.

## Decision: Prevent overlapping periods with a PostgreSQL exclusion constraint

**Rationale**: Application checks alone race. A `btree_gist` exclusion constraint
on `(MemberId, tstzrange(StartDate, EndDate))` guarantees non-overlap at the
database level. The second concurrent purchase fails with SQLSTATE `23P01` on the
named subscription constraint. Only that constraint is translated to
`SubscriptionConflictException` (matched by table and constraint name), so
exclusion violations on other tables are not mislabeled; the translated error is
reported as a conflict per the clarification.

**Alternatives considered**:
- Serializable transaction plus retry: works but is heavier and requires retry
  plumbing in every purchase path.
- Advisory lock per member: works but is less declarative and easy to forget on
  future write paths.

## Decision: Server-side pricing through `ISubscriptionPlanProvider`

**Rationale**: The client sends only plan code and duration; the server resolves
the plan and computes `total = monthly price * duration`. This removes the current
client-controlled price hole and keeps plan data in one place.

`SubscriptionPlans` in `appsettings.json` is the source of truth. Current values
as of this plan: `unlimited` = 1200 UAH/month, `monthly12` = 1000 UAH/month,
`VisitLimitPerMonth` = null and 12 respectively. These amounts are configuration,
not code: tests and docs must read them from the provider or the plans endpoint
and must not hardcode them.

**Alternatives considered**:
- Trusting a client-provided price: rejected as a financial vulnerability.
- Hardcoding a single price: rejected because two plans are already defined and
  their prices are configuration.

## Decision: Payment status is `Completed` at sale; enum ready for later

**Rationale**: There is no external payment provider in scope, so every recorded
payment is completed at the moment of sale. A status field with
`Pending`/`Completed`/`Refunded` keeps the aggregate ready for the future saga and
refund flows without changing the contract now.

**Alternatives considered**:
- No status field at all: would force a schema change when online payments and
  refunds arrive. Rejected.

## Decision: Revenue is the sum of payments by payment date

**Rationale**: The finance source of truth is the payment record, so revenue is
computed from `PaidAt`, not from subscription start dates or plan prices. This
stays correct when prices change or plans are replaced.

**Alternatives considered**:
- Summing subscription period prices by their start date: misattributes money and
  cannot represent multiple payments for one period. Rejected.

## Decision: Read side uses Dapper queries and DTOs

**Rationale**: Payment lists and revenue reports are read-only projections; Dapper
matches the existing read-side convention and avoids tracking overhead.

**Alternatives considered**:
- EF Core for reports: heavier and inconsistent with existing query handlers.
  Rejected.

## Known gap: error status mapping is deferred

**Rationale**: The project has no global exception middleware yet. Command
handlers will throw domain/application exceptions for rejected operations
(missing plan, invalid duration, overlap, cap exceeded). Mapping these to `400`,
`404`, and `409` is deferred with the middleware work. The quickstart validates
rejection behavior; status-code mapping is tracked as technical debt.

**Risk**: Until the middleware lands, a rejected purchase may surface as a server
error. This does not change correctness but must be resolved before public
release.
