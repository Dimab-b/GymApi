# Implementation Plan: Subscription Payments

**Branch**: `001-subscription-payments` | **Date**: 2026-11-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-subscription-payments/spec.md`

## Summary

Add server-priced subscription purchase and renewal with a separate `Payment`
aggregate. Members buy or renew online (payment method `Online`); reception sells
through separate admin endpoints with `Cash` or `Terminal`. Every purchase or
renewal creates a new, non-overlapping subscription period (prepayment allowed up
to 12 months from the first current/future period) and records exactly one payment
atomically. Administrators can list a member's payments and view revenue by date
range with optional method/purpose filters.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`), nullable reference types enabled.

**Primary Dependencies**: ASP.NET Core Web API, MediatR 14.1.0, EF Core 10.0.9,
Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2, Dapper 2.1.79, MassTransit 8.x
(RabbitMQ + EF Core outbox/inbox), Hangfire 1.8.x, MailKit 4.18.2.

**Storage**: PostgreSQL (writes and migrations via EF Core; payment/revenue reads
via Dapper), RabbitMQ broker, Hangfire tables in PostgreSQL.

**Testing**: No test project exists yet. Verification is `dotnet build
GymAndTrainingApi.slnx` plus end-to-end HTTP and broker checks; automated tests
are a known gap (constitution V) and are not introduced by this feature.

**Target Platform**: Linux or Windows server via Docker; local development on
Windows with Docker Compose and native PostgreSQL.

**Project Type**: Web service - modular monolith with Clean Architecture
(`Gym.Domain`, `Gym.Application`, `Gym.Infrastructure`, `GymAndTrainingApi`).

**Performance Goals**: No explicit throughput target. User-facing target from the
spec: a purchase or renewal completes in under 2 minutes. Otherwise standard web
expectations; no hot-path optimization required for the expected gym volume.

**Constraints**: All timestamps UTC (`timestamptz`); subscription periods must not
overlap; prepaid coverage capped at 12 months from the first current/future
period; one currency per plan; no currency conversion; revenue grouped by
currency; payment considered completed at the moment of sale; no external
payment provider, refunds, or invoicing.

**Scale/Scope**: Small/medium single-gym deployment with a handful of configured
plans, moderate member and payment volume. No multi-tenant or FX-conversion
requirements; revenue is grouped by currency.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Layered architecture and dependency direction | PASS | `Payment` lives in `Gym.Domain/Payments`; commands/queries in Application; EF/Dapper in Infrastructure; controllers stay thin. No new project. |
| II. CQRS and aggregate boundaries | PASS | Purchase/renew are records with handlers in the same file; `Payment` is a separate root referencing `Member`/`Subscription` by Id. |
| III. Event-driven integration | PASS | Reuse `SubscriptionPurchased` chain; no integration event published from a command handler; no new consumer side effects required. |
| IV. EF Core and data integrity | PASS (justified deviation) | `Payment` gets its own EF configuration and migration. Purchase writes the Member/Subscription and the Payment aggregate in one transaction - explicitly allowed by constitution IV. Period overlap is enforced by an exclusion constraint. UTC and `ValueGeneratedNever()` are respected. |
| V. Verification, security and git safety | PASS | Verification uses build and e2e; secrets stay in User Secrets; no commit/push without request. |

No unresolved gate failures. The only notable deviation is updating two
aggregates (`Member` and `Payment`) in one transaction, which constitution IV
already sanctions for purchases.

## Project Structure

### Documentation (this feature)

```text
specs/001-subscription-payments/
|-- plan.md          # This file
|-- research.md      # Phase 0 decisions
|-- data-model.md    # Phase 1 entities, constraints, transitions
|-- quickstart.md    # Phase 1 validation guide
|-- contracts/
|   `-- subscription-payments.openapi.yaml
`-- tasks.md         # Created by /speckit.tasks
```

### Source Code (repository root)

```text
Gym.Domain/
|-- Payments/
|   |-- Payment.cs                 # New aggregate root
|   |-- PaymentEvents.cs           # Optional payment event(s)
|   `-- IPaymentRepository.cs
|-- Members/
|   |-- Member.cs                  # Purchase/renew period creation
|   `-- Subscription.cs            # Period entity (no separate snapshot)
`-- Common/ValueObjects/Price.cs

Gym.Application/
|-- Members/Commands/
|   `-- PurchaseSubscriptionCommand.cs   # Shared purchase/renewal orchestration (Member + Payment)
|-- Members/Queries/                     # Member profile/plans read models
|-- Members/Dto's/
|   `-- PurchaseResultDto.cs
|-- Payments/Dto's/
|   |-- PaymentDto.cs
|   `-- RevenueDto.cs
|-- Payments/Queries/
|   |-- GetMemberPaymentsQuery.cs        # New (Dapper)
|   `-- GetRevenueQuery.cs               # New (Dapper)
|-- Common/Interfaces/
|   `-- ISubscriptionPlanProvider.cs     # Existing
`-- Common/Events/Members/
    `-- SubscriptionPurchasedIntegrationEvent.cs

Gym.Infrastructure/
|-- Payments/
|   `-- PaymentRepository.cs
|-- Persistence/
|   |-- AppDbContext.cs                  # DbSet<Payment>
|   `-- Configurations/PaymentConfiguration.cs
`-- Migrations/                          # New migration: Payments + exclusion constraint

GymAndTrainingApi/
|-- Controllers/ForClient/MemberController.cs   # Online purchase/renewal, plans
`-- Controllers/ForBusiness/AdminController.cs  # Cash desk, payments, revenue
```

**Structure Decision**: Keep the existing four-project Clean Architecture and the
existing `Members` feature module for subscription commands/queries, with a new
sibling `Payments` module for the payment aggregate and its repository. No new
projects or top-level patterns are introduced.

## Design Artifacts

- [research.md](./research.md) - key technical decisions and alternatives.
- [data-model.md](./data-model.md) - entities, validation, constraints,
  transitions, and reporting shape.
- [contracts/subscription-payments.openapi.yaml](./contracts/subscription-payments.openapi.yaml)
  - public HTTP contract for client and cash-desk operations.
- [quickstart.md](./quickstart.md) - end-to-end validation scenarios.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Two aggregates (`Member`/`Subscription` and `Payment`) in one transaction | Purchase and payment must be atomic (FR-009) | Eventual consistency via a saga adds compensation and failure modes before the project is split into services; constitution IV explicitly allows this transaction shape for purchases. |
