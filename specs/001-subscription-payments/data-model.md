# Phase 1 Data Model: Subscription Payments

## Entities

### SubscriptionPlan (configuration read model)

Sellable plan data, read from `SubscriptionPlans` configuration through
`ISubscriptionPlanProvider`. Not persisted by this feature.

| Field | Type | Rules |
|-------|------|-------|
| Code | string | Required, unique per configuration, stable business key |
| Name | string | Required |
| PricePerMonth | decimal | Required, greater than 0 |
| Currency | string | Required, exactly 3 letters |
| VisitLimitPerMonth | int? | Optional; null means unlimited. Not enforced by this feature |
| IsAvailable | bool | Optional in configuration; default true. Unavailable plans are not returned and cannot be purchased |

### SubscriptionPeriod (entity inside the Member aggregate)

One purchased or prepaid period. Each purchase or renewal creates a new row; an
existing period is never mutated except when cancelled.

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Domain-assigned, `ValueGeneratedNever()` |
| MemberId | Guid | Required, references Member; not null |
| PlanCode | string | Required, the plan used for this period; max 50 characters |
| VisitLimitPerMonth | int? | Optional; copied from the plan used at purchase; null means unlimited |
| StartDate | DateTime (UTC) | Required; `now` when no current/future period, otherwise the latest existing end date |
| EndDate | DateTime (UTC) | Required; `StartDate + durationMonths` |
| TotalPrice | Price value object | Required; amount actually paid for this period |
| Currency | string | Required, from the plan |

Notes:
- Each purchase or renewal is its own period, so there is no separate snapshot
  entity for a previous plan. The period records the plan code, the total price
  paid, and the plan's visit limit at purchase. Renewal with another plan creates
  a new period with that plan's values.
- Visit-limit enforcement is out of scope for this feature; the stored value is
  available for later enforcement.

### Payment (aggregate root)

One money movement for a subscription period (or a future standalone payment).

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | Domain-assigned, `ValueGeneratedNever()` |
| MemberId | Guid | Required, references Member by Id |
| SubscriptionId | Guid? | References the subscription period by Id; required for purchase/renewal payments |
| Amount | Price value object | Required, value greater than 0; currency from the plan |
| PaidAt | DateTime (UTC) | Required; set at creation |
| Method | PaymentMethod | `Cash`, `Terminal`, `Online` |
| Purpose | PaymentPurpose | `NewSubscription`, `Extension` |
| Status | PaymentStatus | `Pending`, `Completed`, `Refunded`; this feature always creates `Completed` |
| Note | string? | Optional free text (for example a cash-desk reference) |
| IdempotencyKey | Guid? | Client-generated key; unique per member when present |
| IdempotencyRequestHash | string? | SHA-256 hex of member, plan, duration, method, and purpose; detects key reuse with a different payload |

## Enums

- `PaymentMethod`: `Cash`, `Terminal`, `Online`.
- `PaymentPurpose`: `NewSubscription`, `Extension`.
- `PaymentStatus`: `Pending`, `Completed`, `Refunded`.

## Relationships

- `Member` 1 .. * `SubscriptionPeriod` (period rows belong to one member; members
  may have zero or more).
- `Member` 1 .. * `Payment` (payments belong to one member).
- `Payment` * .. 0..1 `SubscriptionPeriod` (purchase/renewal payments reference
  the period they paid for; a payment can exist without a period only in future
  non-subscription use cases).

Aggregates reference each other by `Id` only; no object navigation between
`Member` and `Payment`.

## Validation rules

- Duration must be between 1 and 12 months inclusive.
- Total must equal `PricePerMonth * DurationMonths`.
- A purchase or renewal must be rejected when the plan is missing or inactive.
- A banned member must not purchase or renew.
- Total prepaid coverage must not exceed 12 months from the start of the earliest
  current or future period.
- Concurrent conflicting purchases: the later request is rejected; no period is
  created.
- Payment amount must be greater than 0 and use the plan currency.
- `PlanCode` must be non-empty and at most 50 characters.

## Constraints and indexes

- `Subscriptions.MemberId` is required (non-null) and cascades on member delete.
- `Payments.MemberId` is required.
- `Payments.SubscriptionId` is nullable and indexed.
- `Payments.PaidAt` is indexed for revenue range queries.
- `Payments.Method` and `Payments.Purpose` are indexed only if revenue filtering
  proves slow; not required initially.
- Unique filtered index `(MemberId, IdempotencyKey)` where `IdempotencyKey IS NOT NULL`;
  replaying the same key with the same payload returns the stored result, while
  a different payload is rejected as `idempotency.conflict`.
- PostgreSQL exclusion constraint on `Subscriptions`:
  `EXCLUDE USING gist ("MemberId" WITH =, tstzrange("StartDate", "EndDate") WITH &&)`
  backed by the `btree_gist` extension. Only a violation of this named constraint
  (`23P01`, table `Subscriptions`, constraint
  `EX_Subscriptions_MemberId_Period`) is reported as a conflict; other exclusion
  violations are not reclassified.
- All Guid primary keys are domain-assigned: `ValueGeneratedNever()`.
- All date/time columns are `timestamptz` and written in UTC.
- Date-only filters (revenue `from`/`to`, booking admin range, trainer
  availability `Date`) are interpreted in `Gym:TimeZone` (default
  `Europe/Kyiv`) and converted to UTC boundaries; stored instants remain UTC.

## State transitions

### SubscriptionPeriod

- Created as a future or current period.
- Active while `StartDate <= now <= EndDate`.
- Expired when `EndDate < now`.
- Cancelled by setting `EndDate` to `now` (active period) or `StartDate` (future
  period); cancelled periods are never active.
- Periods never change start date and never overlap.

### Payment

- Created as `Completed` in this feature (payment taken at sale).
- Future flows: `Pending -> Completed`, `Completed -> Refunded`,
  `Pending -> Refunded`. Not implemented here.

## Read models (Dapper)

### PaymentDto

`Id`, `MemberId`, `SubscriptionId`, `Amount`, `Currency`, `PaidAt`, `Method`,
`Purpose`, `Status`, `Note`.

### RevenueDto

- `From`, `To` (echoed range)
- `Totals`: list of `{ Currency, Amount }`, one entry per payment currency

Revenue is grouped by payment currency for payments whose `PaidAt` falls in the
inclusive date range, optionally filtered by `Method` and `Purpose`. An empty
range returns an empty `Totals` list. No currency conversion is performed.
