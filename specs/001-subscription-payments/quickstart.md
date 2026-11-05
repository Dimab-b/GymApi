# Quickstart: Subscription Payments

## Prerequisites

- .NET 10 SDK.
- PostgreSQL reachable at `localhost:5432` with the gym database.
- RabbitMQ reachable at `localhost:5672` (Docker is fine).
- User Secrets set for the API project:
  - `ConnectionStrings:DefaultConnection`
  - `RabbitMq:Host`, `RabbitMq:Username`, `RabbitMq:Password`
  - `EmailSettings:*` (needed by the existing consumers)
- A registered member with a known `MemberId`.
- At least one plan in `SubscriptionPlans` (`unlimited`, `monthly12` by default).

Do not put secrets in `appsettings.json`, `.env`, or any tracked file.

## 1. Build

```powershell
dotnet build GymAndTrainingApi.slnx
```

Expected: build succeeds with 0 errors.

## 2. Apply migrations

```powershell
dotnet ef database update --project Gym.Infrastructure --startup-project GymAndTrainingApi/Gym.Api.csproj
```

Expected: the `Payments` table and the subscription exclusion constraint are
created; no pending model changes remain.

## 3. Run the API

```powershell
dotnet run --project GymAndTrainingApi/Gym.Api.csproj --launch-profile http
```

## 4. List plans

```powershell
curl "http://localhost:5268/Member/subscription-plans"
```

Expected: the configured plans with `pricePerMonth`, `currency`, and optional
`visitLimitPerMonth`; no client-supplied price is used anywhere.

## 5. Online purchase

Set `$PURCHASE_KEY` to a new GUID. Reuse it only for retries of this purchase.

```powershell
curl -X POST "http://localhost:5268/Member/$MEMBER_ID/profile/subscriptions/purchase" `
  -H "Content-Type: application/json" `
  -H "Idempotency-Key: $PURCHASE_KEY" `
  -d '{"planCode":"unlimited","durationMonths":3}'
```

Expected:
- A new period starting now with `EndDate = StartDate + 3 months`.
- `TotalPrice = 3 * plan price per month` in the plan currency.
- Exactly one `Payment` with `Method = Online`, `Purpose = NewSubscription`,
  `Status = Completed`.
- The period and payment are committed together.
- Retrying with the same key and body returns the same `subscriptionId` and does
  not create another payment.

## 6. Online renewal with a different plan

Set `$RENEWAL_KEY` to a new GUID (a new logical operation, so not the purchase key).

```powershell
curl -X POST "http://localhost:5268/Member/$MEMBER_ID/profile/subscriptions/extend" `
  -H "Content-Type: application/json" `
  -H "Idempotency-Key: $RENEWAL_KEY" `
  -d '{"planCode":"monthly12","durationMonths":2}'
```

Expected:
- A new period starting at the previous period's `EndDate` (no overlap, no gap).
- The new period records `monthly12` and the current `monthly12` price.
- The previous period is unchanged.
- Exactly one new `Payment` with `Purpose = Extension`, `Method = Online`.
- Retrying with the same key and body returns the same `subscriptionId` and does
  not create another payment.

## 7. Prepayment cap

Create enough future periods so the total coverage would exceed 12 months from
the start of the earliest current/future period, then attempt one more purchase.

Expected: the request is rejected with HTTP 409 (`code: subscription.coverage_exceeded`)
and no period or payment is created. The cap is measured from the earliest
current/future period start, not from today.

## 8. Concurrent purchase

Send two purchases for the same member at the same time.

Expected: one succeeds with HTTP 200; the other is rejected with HTTP 409
(`code: subscription.period_conflict`) and creates no period or payment. In the
database, the exclusion constraint also prevents overlapping rows.

## 9. Cash-desk sale

Set `$CASH_KEY` to a new GUID. The header is required for cash-desk operations.

```powershell
curl -X POST "http://localhost:5268/Admin/members/$MEMBER_ID/subscriptions/purchase" `
  -H "Content-Type: application/json" `
  -H "Idempotency-Key: $CASH_KEY" `
  -d '{"planCode":"monthly12","durationMonths":1,"method":"Cash"}'
```

Expected: the same period/payment behavior as online purchase, with
`Method = Cash` (or `Terminal`). A missing method or `Online` is rejected with
HTTP 400 (`code: request.invalid`). A missing `Idempotency-Key` is rejected with
HTTP 400 (`code: idempotency.key_required`).

## 10. Member payments

```powershell
curl "http://localhost:5268/Admin/members/$MEMBER_ID/payments?pageNumber=1&pageSize=20"
```

Expected: the member's payments newest first, with amount, currency, method,
purpose, status, and timestamps. An unknown member returns HTTP 200 with an
empty page.

## 11. Revenue

```powershell
curl "http://localhost:5268/Admin/revenue?from=2026-11-01&to=2026-11-30"
curl "http://localhost:5268/Admin/revenue?from=2026-11-01&to=2026-11-30&method=Cash"
curl "http://localhost:5268/Admin/revenue?from=2026-11-01&to=2026-11-30&purpose=Extension"
```

Expected:
- `from`/`to` are local dates in `Gym:TimeZone` (default `Europe/Kyiv`);
  stored timestamps remain UTC.
- Revenue is grouped by currency; each entry equals the sum of payments in that
  currency with `PaidAt` in the inclusive range.
- Method and purpose filters narrow the result.
- A range with no payments returns `totals: []`.

## 12. Ban and renewal

Ban the member, then attempt a purchase and a renewal.

Expected: both are rejected with HTTP 409 (`code: member.banned`); no period or
payment is created, and existing current/future periods are cancelled by the ban.

## 13. Database checks

```sql
SELECT * FROM "Payments" ORDER BY "PaidAt" DESC;
SELECT "MemberId", "StartDate", "EndDate", "PlanCode", "PriceAmount", "PriceCurrency"
FROM "Subscriptions"
ORDER BY "StartDate";
```

Expected: no overlapping ranges for the same member, every purchase/renewal has a
matching payment, and all timestamps are UTC.

## 14. Idempotency checks

Repeat the online purchase from step 5 with the same `$PURCHASE_KEY` and the same
body.

Expected: HTTP 200 with the same `subscriptionId`; the payment count does not
change.

Repeat with the same key but a different duration (for example
`durationMonths: 1`).

Expected: HTTP 409 (`code: idempotency.conflict`); no new period or payment.

Call the cash-desk purchase from step 9 without `Idempotency-Key`.

Expected: HTTP 400 (`code: idempotency.key_required`).

Repeat the cash-desk sale with the same `$CASH_KEY` and body.

Expected: HTTP 200 with the same `subscriptionId`; the payment count does not
change.
