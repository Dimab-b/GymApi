# Tasks: Subscription Payments

**Input**: Design documents from `/specs/001-subscription-payments/`

**Prerequisites**: `plan.md` (required), `spec.md` (required), `research.md`,
`data-model.md`, `contracts/`, `quickstart.md`

**Tests**: No test project exists in this repository, and `spec.md` does not
request TDD. This task list does not add a test project. Verification is
`dotnet build` plus the end-to-end scenarios in `quickstart.md`.

**Organization**: Tasks are grouped by user story so each story can be
implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: `US1`, `US2`, `US3` (user story phases only)
- Every task includes an exact repository file path or command

## Path Conventions

This is a four-project Clean Architecture solution:

- `Gym.Domain/` - entities, value objects, domain events, repository interfaces
- `Gym.Application/` - MediatR commands/queries, DTOs, event bridges
- `Gym.Infrastructure/` - EF Core, Dapper queries, consumers, migrations
- `GymAndTrainingApi/` - ASP.NET Core controllers (`Gym.Api.csproj`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the baseline and the configuration values the feature relies on.

- [X] T001 Run `dotnet build GymAndTrainingApi.slnx` from the repository root and confirm it succeeds; confirm `GymAndTrainingApi/appsettings.json` `SubscriptionPlans` contains `unlimited` at 1200 UAH/month and `monthly12` at 1000 UAH/month; do not hardcode these amounts anywhere in code.
- [X] T002 [P] Run `dotnet ef --version` and confirm the EF tooling is available; `Microsoft.EntityFrameworkCore.Design` and `Microsoft.EntityFrameworkCore.Tools` are already referenced by `Gym.Infrastructure/Gym.Infrastructure.csproj` and `GymAndTrainingApi/Gym.Api.csproj`, so no package changes are required.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Payment aggregate, persistence, database guard, and the domain rules
shared by every purchase path.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 [P] Create the payment enums in `Gym.Domain/Payments/PaymentEnums.cs` with exactly these values: `PaymentMethod { Cash, Terminal, Online }`, `PaymentPurpose { NewSubscription, Extension }`, `PaymentStatus { Pending, Completed, Refunded }`.
- [X] T004 Create the `Payment` aggregate root in `Gym.Domain/Payments/Payment.cs` with `Id` (Guid), `MemberId` (Guid), `SubscriptionId` (Guid?), `Amount` (`Price`), `PaidAt` (DateTime, UTC), `Method`, `Purpose`, `Status`, and `Note` (string?); add a private parameterless constructor for EF and a static `Create(Guid memberId, Guid? subscriptionId, Price amount, PaymentMethod method, PaymentPurpose purpose, string? note = null)` that assigns `Id = Guid.NewGuid()`, `PaidAt = DateTime.UtcNow`, and `Status = PaymentStatus.Completed`; implement `IAggregateRoot`; depends on T003.
- [X] T005 Create `Gym.Domain/Payments/IPaymentRepository.cs` with `void Add(Payment payment);`; depends on T004.
- [X] T006 Create `Gym.Infrastructure/Payments/PaymentRepository.cs` implementing `IPaymentRepository` by calling `AppDbContext.Payments.Add(payment)`; depends on T005.
- [X] T007 Add `DbSet<Payment> Payments` to `Gym.Infrastructure/Persistence/AppDbContext.cs`; depends on T004.
- [X] T008 Create `Gym.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs`: map to table `Payments`; key `Id` with `ValueGeneratedNever()`; `OwnsOne` `Amount` to columns `AmountValue` (`decimal(18,2)`, required) and `AmountCurrency` (`varchar(3)`, required); convert `Method`, `Purpose`, and `Status` to strings with max length 20 and required; mark `PaidAt` required; `Note` max length 500; add indexes on `MemberId`, `SubscriptionId`, and `PaidAt`; depends on T004.
- [X] T009 Register `IPaymentRepository` in `Gym.Infrastructure/DependencyInjection.cs` as `services.AddScoped<IPaymentRepository, PaymentRepository>();`; depends on T006.
- [X] T010 [P] Create `Gym.Domain/Members/SubscriptionConflictException.cs` as a `sealed class` deriving from `Exception` with a constructor accepting a message.
- [X] T011 Override `SaveChangesAsync(CancellationToken)` in `Gym.Infrastructure/Persistence/AppDbContext.cs` to catch `DbUpdateException` whose inner exception is `Npgsql.PostgresException` and translate it to `SubscriptionConflictException` only when all three match: `SqlState == PostgresErrorCodes.ExclusionViolation` (`23P01`), `TableName == "Subscriptions"`, and `ConstraintName == "EX_Subscriptions_MemberId_Period"`; rethrow every other `23P01` unchanged so exclusion violations on other tables are not mislabeled as subscription conflicts; do not swallow the exception and let the transaction roll back; depends on T010 and must be applied after T007 in the same file.
- [X] T012 Update `Gym.Domain/Members/Member.cs`: in `PurchaseSubscription(string planCode, decimal pricePerMonth, string currency, int durationMonths, int? visitLimitPerMonth)`, reject `durationMonths < 1 || durationMonths > 12`; compute `anchor` as the minimum `StartDate` of subscriptions with `EndDate > now` (or `now` when none) and reject when the new period end is later than `anchor.AddMonths(12)`; return the created `Subscription` instead of `void`; keep the existing banned check and `SubscriptionPurchasedDomainEvent`; keep the legacy `PurchaseSubscription(int durationMonths, Price monthlyPrice)` overload until T020.
- [X] T013 Generate the migration with `dotnet ef migrations add AddSubscriptionPayments --project Gym.Infrastructure --startup-project GymAndTrainingApi/Gym.Api.csproj`; review the new file under `Gym.Infrastructure/Migrations/`; depends on T007, T008, T011, and T012.
- [X] T014 Edit the generated `Gym.Infrastructure/Migrations/*_AddSubscriptionPayments.cs`: in `Up`, add `migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;")` and the exclusion constraint `ALTER TABLE "Subscriptions" ADD CONSTRAINT "EX_Subscriptions_MemberId_Period" EXCLUDE USING gist ("MemberId" WITH =, tstzrange("StartDate", "EndDate") WITH &&);`; in `Down`, drop `"EX_Subscriptions_MemberId_Period"` but do not drop the `btree_gist` extension; before applying, run this read-only check and stop if it returns rows: `SELECT a."MemberId", a."Id", b."Id" FROM "Subscriptions" a JOIN "Subscriptions" b ON a."MemberId" = b."MemberId" AND a."Id" < b."Id" AND tstzrange(a."StartDate", a."EndDate") && tstzrange(b."StartDate", b."EndDate");`; depends on T013.
- [X] T015 Apply the migration with `dotnet ef database update --project Gym.Infrastructure --startup-project GymAndTrainingApi/Gym.Api.csproj`; verify the `Payments` table and `EX_Subscriptions_MemberId_Period` exist; depends on T014.

**Checkpoint**: The Payment aggregate, table, repository, conflict translation, and
domain purchase rules are ready. The build still succeeds because the legacy
overload and the old commands remain in place.

---

## Phase 3: User Story 1 - Member buys or renews online (Priority: P1) - MVP

**Goal**: A member selects a plan code and duration, the server prices it, and the
system records one new subscription period plus one Online payment atomically.
Renewal may use any plan and starts at the latest period end.

**Independent Test**: With no subscription, `POST
/Member/{id}/profile/subscriptions/purchase` with `{"planCode":"unlimited","durationMonths":3}`
creates a period whose total is 3 x the configured plan price and one Online
payment; a follow-up `extend` with `{"planCode":"monthly12","durationMonths":2}`
starts at the previous end date, uses the `monthly12` price, and does not overlap.

### Implementation for User Story 1

- [X] T016 [P] [US1] Create `Gym.Application/Members/Dto's/PurchaseResultDto.cs` with `Guid SubscriptionId`, `decimal Amount`, and `string Currency`.
- [X] T017 [US1] Rewrite `Gym.Application/Members/Commands/PurchaseSubscriptionCommand.cs` as the shared command: `record PurchaseSubscriptionCommand(Guid MemberId, string PlanCode, int DurationMonths, PaymentMethod Method, PaymentPurpose Purpose) : IRequest<PurchaseResultDto>`; in the handler reject an empty `PlanCode`, invalid enum values, and resolve the plan with `ISubscriptionPlanProvider.GetByCode` (missing plan throws `InvalidOperationException`); load the member with `IMemberRepository.GetByIdAsync` (missing member throws `KeyNotFoundException`); call `member.PurchaseSubscription(plan.Code, plan.PricePerMonth, plan.Currency, command.DurationMonths, plan.VisitLimitPerMonth)`; create `Payment.Create(member.Id, subscription.Id, subscription.Price, command.Method, command.Purpose)` and call `IPaymentRepository.Add`; call `IUnitOfWork.SaveChangesAsync` exactly once so the period, payment, and outbox message commit together; return `new PurchaseResultDto(subscription.Id, subscription.Price.Value, subscription.Price.Currency)`; depends on T004, T005, T006, T009, T012, and T016.
- [X] T018 [US1] Update `GymAndTrainingApi/Controllers/ForClient/MemberController.cs`: change `PurchaseSubscriptionRequest` and `ExtendSubscriptionRequest` to `(string PlanCode, int DurationMonths)`; send `PaymentMethod.Online` with `PaymentPurpose.NewSubscription` for purchase and `PaymentPurpose.Extension` for extend; return `Ok(PurchaseResultDto)`; the request has no method or price field, so a client cannot choose Cash/Terminal or a price; add `using Gym.Domain.Payments;`; depends on T017.
- [X] T019 [US1] Delete `Gym.Application/Members/Commands/ExtendSubscriptionCommand.cs` and remove its remaining references (the extend endpoint now sends `PurchaseSubscriptionCommand`); depends on T018.
- [X] T020 [US1] Remove the legacy `PurchaseSubscription(int durationMonths, Price monthlyPrice)` overload from `Gym.Domain/Members/Member.cs`; depends on T017 and T019.

**Checkpoint**: US1 is fully functional and independently testable. Online
purchase and renewal use server pricing, any plan, the 12-month cap, and one
Online payment per operation.

---

## Phase 4: User Story 2 - Reception sells a subscription (Priority: P1)

**Goal**: Reception staff sells the same subscription/renewal through admin
endpoints and records Cash or Terminal payments.

**Independent Test**: `POST /Admin/members/{id}/subscriptions/purchase` with
`{"planCode":"monthly12","durationMonths":1,"method":"Cash"}` creates the same
period and payment shape as online, with `Method = Cash`; repeating with
`Terminal` records `Terminal`; sending `Online` is rejected.

### Implementation for User Story 2

- [X] T021 [US2] Update `GymAndTrainingApi/Controllers/ForBusiness/AdminController.cs`: add `CashDeskPurchaseRequest(string PlanCode, int DurationMonths, PaymentMethod? Method)` so a missing method is not silently treated as `Cash`; add `POST members/{id:guid}/subscriptions/purchase` and `POST members/{id:guid}/subscriptions/extend`; reject `Method` that is null or not `Cash`/`Terminal` with `ArgumentException`; send the shared `PurchaseSubscriptionCommand` with `request.Method.Value` and `PaymentPurpose.NewSubscription` or `PaymentPurpose.Extension`; return `Ok(PurchaseResultDto)`; add `using Gym.Domain.Payments;`; depends on T017.

**Checkpoint**: US1 and US2 are both functional. They share one command; the only
difference is the endpoint and the payment method.

---

## Phase 5: User Story 3 - Administrator reviews payments and revenue (Priority: P2)

**Goal**: Administrators list a member's payments and view revenue for a date
range, optionally filtered by method or purpose.

**Independent Test**: After sales, `GET /Admin/members/{id}/payments` shows
amount, date, method, and purpose for each payment; `GET /Admin/revenue` for the
same range returns the sum of those payments, and method/purpose filters narrow
the total correctly.

### Implementation for User Story 3

- [X] T022 [P] [US3] Create `Gym.Application/Payments/Dto's/PaymentDto.cs` with `Guid Id`, `Guid MemberId`, `Guid? SubscriptionId`, `decimal Amount`, `string Currency`, `DateTime PaidAt`, `string Method`, `string Purpose`, `string Status`, and `string? Note` (read model uses strings, matching the API contract).
- [X] T023 [P] [US3] Create `Gym.Application/Payments/Dto's/RevenueDto.cs` with `DateOnly From`, `DateOnly To`, and `IReadOnlyList<RevenueTotalDto> Totals`; `RevenueTotalDto` has `string Currency` and `decimal Amount`.
- [X] T024 [US3] Implement `Gym.Application/Payments/Queries/GetMemberPaymentsQuery.cs` with `record GetMemberPaymentsQuery(Guid MemberId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<PaymentDto>>`; use Dapper and `IConfiguration.GetConnectionString("DefaultConnection")`; reject `PageNumber < 1 || PageNumber > 100000` and `PageSize < 1 || PageSize > 100`; do not verify member existence (unknown member returns an empty page); query `"Payments"` ordered by `"PaidAt" DESC, "Id" DESC` with `LIMIT @PageSize OFFSET @Offset` and a `long` offset; map `"AmountValue"` to `Amount`, `"AmountCurrency"` to `Currency`, and keep `"Method"`/`"Purpose"`/`"Status"` as strings; return `PagedResult<PaymentDto>`; depends on T022.
- [X] T025 [US3] Implement `Gym.Application/Payments/Queries/GetRevenueQuery.cs` with `record GetRevenueQuery(DateOnly From, DateOnly To, PaymentMethod? Method = null, PaymentPurpose? Purpose = null) : IRequest<RevenueDto>`; reject `From > To` and `To == DateOnly.MaxValue`; convert to a UTC inclusive-lower/exclusive-upper range (`From` at 00:00 UTC, `To + 1 day` at 00:00 UTC); group `SUM("AmountValue")` by `"AmountCurrency"`; compare the string enum columns using `Method?.ToString()` and `Purpose?.ToString()` with explicit `::text` casts, for example `(@Method::text IS NULL OR "Method" = @Method)`, so Npgsql can infer the parameter type when the filter is null; return `RevenueDto` with one `RevenueTotalDto` per currency (empty list when there are no payments); depends on T023.
- [X] T026 [US3] Update `GymAndTrainingApi/Controllers/ForBusiness/AdminController.cs`: add `GET members/{id:guid}/payments` with `[FromQuery] int pageNumber = 1` and `[FromQuery] int pageSize = 20`, sending `GetMemberPaymentsQuery`; add `GET revenue` with `[FromQuery] DateOnly from`, `[FromQuery] DateOnly to`, `[FromQuery] PaymentMethod? method`, and `[FromQuery] PaymentPurpose? purpose`, sending `GetRevenueQuery`; return the results with `Ok`; depends on T024 and T025.

**Checkpoint**: All three user stories work. Revenue is computed from payments,
not from subscription dates or prices.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verification and safety checks across the feature.

- [X] T027 Run `dotnet build GymAndTrainingApi.slnx` and fix all errors and warnings introduced by this feature.
- [X] T028 Run `dotnet ef migrations has-pending-model-changes --project Gym.Infrastructure --startup-project GymAndTrainingApi/Gym.Api.csproj` and confirm it reports no pending changes.
- [X] T029 Execute the scenarios in `specs/001-subscription-payments/quickstart.md`: online purchase and renewal, plan switch, 12-month cap, concurrent purchase conflict, cash and terminal sales, banned member, member payments, and revenue with filters; record the actual results.
- [X] T030 Review `git diff` and `git status`: confirm no secrets, `.env`, connection strings, or old hardcoded prices (600/400) are present; do not commit or push unless explicitly requested.
- [X] T031 Verify the implemented routes, request bodies, and response shapes match `specs/001-subscription-payments/contracts/subscription-payments.openapi.yaml` (200 responses, `PurchaseResultDto`, grouped revenue totals); update the contract only for a deliberate deviation.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational.
- **User Story 2 (Phase 4)**: Depends on Foundational and on T017 (the shared command from US1).
- **User Story 3 (Phase 5)**: Depends on Foundational for the `Payments` table; its independent test needs payments produced by US1 or US2.
- **Polish (Phase 6)**: Depends on all desired stories.

### User Story Dependencies

- **US1 (P1)**: The MVP revenue path; can start after Foundational.
- **US2 (P1)**: Reuses the T017 command; after T017 it touches a different
  controller file (`AdminController.cs`) than US1 and can proceed in parallel with
  T018-T020.
- **US3 (P2)**: New query files and `AdminController.cs`; after Foundational it can
  be implemented in parallel with US1/US2, but its end-to-end check needs at least
  one sale from US1 or US2.

### Within Each User Story

- DTOs before handlers that return them.
- The shared command (T017) before both controllers.
- Controllers before deleting the old command/overload (T019, T020).
- Queries (T024, T025) before the controller endpoints (T026).

### Parallel Opportunities

- Setup: T001 and T002 are independent read-only checks.
- Foundational: T003, T010, and T012 touch different files and can run together.
  T004, T005, T006, T008, and T009 form a dependency chain; T007 and T011 touch
  the same `AppDbContext.cs` and must be sequential.
- US1: T016 can run before or alongside T017.
- US3: T022 and T023 are independent DTO files; T024 and T025 are independent
  query files.
- After T017, US1 controller work (`MemberController.cs`) and US2 controller work
  (`AdminController.cs`) are different files and can run in parallel.

---

## Parallel Example: User Story 3

```text
Task: "Create PaymentDto in Gym.Application/Payments/Dto's/PaymentDto.cs"
Task: "Create RevenueDto in Gym.Application/Payments/Dto's/RevenueDto.cs"

# After both DTOs exist:
Task: "Implement GetMemberPaymentsQuery in Gym.Application/Payments/Queries/GetMemberPaymentsQuery.cs"
Task: "Implement GetRevenueQuery in Gym.Application/Payments/Queries/GetRevenueQuery.cs"
```

## Parallel Example: After the Shared Command

```text
# T017 is complete; both endpoints are independent files:
Task: "Update MemberController in GymAndTrainingApi/Controllers/ForClient/MemberController.cs"
Task: "Add cash-desk endpoints in GymAndTrainingApi/Controllers/ForBusiness/AdminController.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: US1 (online purchase and renewal).
4. Stop and validate US1 with the quickstart scenarios.
5. Deploy or demo the online revenue path.

### Incremental Delivery

1. Setup + Foundational -> payment aggregate, table, and shared domain rules.
2. US1 -> online purchase/renewal with server pricing and payment (MVP).
3. US2 -> cash-desk sales with Cash/Terminal; same command, different endpoint.
4. US3 -> payment list and revenue reporting.
5. Run the Polish phase after every increment.

### Notes

- Do not add a test project in this feature; the repository has none and the
  specification does not request TDD.
- The 12-month cap is measured from the earliest current/future period start, not
  from today; keep the domain method as the single place for this rule.
- Do not publish integration events from the command handler. The existing
  `SubscriptionPurchasedDomainEvent` bridge and EF outbox remain the only path.
- HTTP mapping for domain rejections (400/404/409) is deferred with the global
  exception middleware, as recorded in `research.md`; correctness is verified by
  the absence of a period/payment row. Do not add ad-hoc exception middleware in
  this feature.
