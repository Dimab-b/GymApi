# Feature Specification: Subscription Payments

**Feature Branch**: `001-subscription-payments`

**Created**: 2026-11-02

**Status**: Implemented

**Implementation**: Complete — 31/31 tasks; e2e verified (SC-001 purchase/renewal completed in seconds, SC-002 server pricing, SC-003 no overlapping periods under concurrency, SC-004 revenue matched payments, SC-005 no client price, SC-006 atomic rollback, SC-007 12-month cap).

**Input**: User description: "Додати оплату підписок. Член/адмін купує або продовжує підписку за планом; ціну визначає сервер (SubscriptionPlan), клієнт передає лише PlanCode і DurationMonths. Кожна покупка/продовження створює новий період підписки (передоплата дозволена) і запис платежу з методом Cash/Terminal/Online. Адмін може дивитись платежі члена та виручку за період. Клієнтські та касові операції — різні ендпоінти, одна команда."

## Clarifications

### Session 2026-11-02

- Q: Що робити з другим запитом на купівлю/продовження, якщо два запити для одного члена надходять одночасно? → A: Відхилити другий запит із конфліктом; період для нього не створюється.
- Q: Від якої дати рахувати обмеження «не більше ніж на рік» для передоплати? → A: Від дати початку найранішого поточного або майбутнього періоду; сумарне покриття не може перевищувати 12 місяців від цієї дати.
- Q: При продовженні член обирає новий план чи поточний? → A: Будь-який доступний план; кожна покупка або продовження оформлюється окремим періодом, без окремого снапшота.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Member buys or renews a subscription online (Priority: P1)

A registered member selects an available plan and a number of months, pays online, and receives an active subscription period. If the member already has an active or prepaid period, the new period is appended after it (prepayment) and never overlaps.

**Why this priority**: This is the primary revenue path and the main reason the feature exists. Without it, no subscription can be sold to a member.

**Independent Test**: A member with no subscription selects a plan and duration; the system shows a server-calculated total, records a payment marked as online, and creates a new active period. A second request while the period is active creates a prepaid period starting at the current end date.

**Acceptance Scenarios**:

1. **Given** an available plan with a monthly price, **When** a member buys it for 3 months, **Then** the total equals the plan monthly price multiplied by 3, a new period is created, and a payment for that total is recorded as online.
2. **Given** a member with an active period ending in the future, **When** the member renews with any available plan, **Then** the new period starts at the current end date, does not overlap, and is paid at the chosen plan's current price.
3. **Given** a member provides a price in the request, **When** the purchase is processed, **Then** the provided price is not used; only the server-side plan price applies.
4. **Given** the selected plan is unavailable, **When** the member tries to buy it, **Then** the purchase is rejected with a clear error.
5. **Given** the selected duration is outside the allowed range, **When** the purchase is processed, **Then** the purchase is rejected.
6. **Given** a member whose first current or future period started 10 months ago, **When** the member tries to prepay a period that would extend coverage past 12 months from that start, **Then** the purchase is rejected with a clear error.

---

### User Story 2 - Reception sells a subscription or renewal (Priority: P1)

Reception staff sells a subscription or renewal to a member at the gym, choosing cash or terminal as the payment method. The same subscription and payment rules apply as online, but the operation is performed by an administrator through cash-desk endpoints.

**Why this priority**: Most gym sales happen at reception, including for walk-in visitors, so the cash-desk flow is essential for day-to-day operations.

**Independent Test**: An administrator selects a member, plan, and duration, records cash payment, and the member receives a new period with a payment marked as cash; repeating with terminal records the terminal method.

**Acceptance Scenarios**:

1. **Given** an administrator and a member with no active period, **When** the administrator sells a plan for cash, **Then** a period starting now and a payment marked as cash are recorded.
2. **Given** the administrator selects terminal, **When** the sale completes, **Then** the payment is recorded as terminal.
3. **Given** a member uses the online purchase endpoint, **When** the request tries to set a cash or terminal method, **Then** the method is not honored (online only).
4. **Given** a banned member, **When** any purchase or renewal is attempted, **Then** it is rejected.

---

### User Story 3 - Administrator reviews payments and revenue (Priority: P2)

An administrator can list a member's payments and view revenue for a date range, optionally filtered by payment method or purpose, to reconcile the cash desk and understand income.

**Why this priority**: Financial visibility is needed to verify that sales and revenue match, but it depends on the sale flows from stories 1 and 2.

**Independent Test**: After several sales, an administrator lists a member's payments and requests revenue for the period; the totals equal the sum of the recorded payments and respect the chosen filters.

**Acceptance Scenarios**:

1. **Given** a member with payments, **When** the administrator requests that member's payments, **Then** each payment shows amount, date, method, and purpose.
2. **Given** payments across a date range, **When** the administrator requests revenue for that range, **Then** the reported total equals the sum of payments whose payment date is inside the range.
3. **Given** optional method and purpose filters, **When** applied, **Then** only matching payments are included in the total.

---

### Edge Cases

- The plan is missing or inactive at the moment of purchase or renewal.
- The requested duration is below 1 month or above the allowed maximum.
- The member is banned.
- Two purchases happen concurrently for the same member; the second is rejected with a conflict and no period is created, so periods never overlap.
- A renewal is made before the current period ends (prepayment) or after it expired (starts now).
- A prepayment would extend total coverage more than 12 months from the start of the member's first current or future period.
- A client attempts to send a server-only field such as price or a cash-desk payment method.
- Recording the period succeeds but recording the payment fails (or vice versa); neither must remain.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST present available subscription plans with their monthly price and visit limit (if any).
- **FR-002**: A member purchase or renewal request MUST identify the plan and duration only; the client MUST NOT be able to set the price.
- **FR-003**: The system MUST compute the total as the plan monthly price multiplied by the requested duration.
- **FR-004**: The system MUST accept durations from 1 to 12 months inclusive and reject anything outside that range.
- **FR-005**: Each purchase or renewal MUST create a new subscription period with its own start date, end date, plan used, and total price paid.
- **FR-006**: If the member already has a current or future period, a new period MUST start at the latest existing end date so periods never overlap; prepayment is allowed.
- **FR-007**: If the member has no current or future period, a new period MUST start at the current time.
- **FR-008**: Each purchase or renewal MUST record exactly one payment with amount, payment date, method (cash, terminal, or online), purpose (new subscription or renewal), and status.
- **FR-009**: Subscription period creation and payment recording MUST be atomic: both are saved or neither is.
- **FR-010**: Online member operations MUST record the payment method as online; cash-desk operations MUST accept only cash or terminal.
- **FR-011**: Client and cash-desk operations MUST remain separate operations from the caller's perspective while sharing the same underlying business operation.
- **FR-012**: If the plan is unavailable, the system MUST reject the purchase or renewal with a clear error.
- **FR-013**: A banned member MUST NOT be able to purchase or renew.
- **FR-014**: Subscription periods for the same member MUST NOT overlap.
- **FR-015**: Administrators MUST be able to list a member's payments with amount, date, method, and purpose.
- **FR-016**: Administrators MUST be able to view revenue for a date range, optionally filtered by method and purpose, where revenue equals the sum of payments in that range.
- **FR-017**: When concurrent purchase or renewal requests for the same member conflict, the system MUST reject the later request with a conflict error and MUST NOT create a period for it.
- **FR-018**: Total prepaid coverage MUST NOT extend more than 12 months from the start of the member's first current or future period; a purchase or renewal that would exceed this limit MUST be rejected.

### Key Entities *(include if feature involves data)*

- **Subscription Plan**: A sellable offer with a code, name, monthly price, currency, optional monthly visit limit, and availability state.
- **Subscription Period**: One purchased or prepaid period for a member with start date, end date, plan used, total price paid, and currency. Periods for the same member never overlap.
- **Payment**: A record of money received for a subscription period, with amount, currency, payment date, method (cash, terminal, online), purpose (new subscription or renewal), and status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A member or administrator can complete a purchase or renewal in under 2 minutes.
- **SC-002**: 100% of recorded payments equal the plan monthly price multiplied by the purchased duration.
- **SC-003**: Zero subscription periods overlap for the same member, including under concurrent purchases.
- **SC-004**: The revenue report matches the sum of recorded payments in the selected period with 100% accuracy.
- **SC-005**: Zero purchases accept a price supplied by the client.
- **SC-006**: 100% of purchases and renewals leave either both a period and a payment or neither.
- **SC-007**: Zero subscription periods extend total coverage more than 12 months from the start of the member's first current or future period.

## Assumptions

- Plans are predefined and few in number; administrators do not manage them day to day in this feature.
- The allowed duration range is 1 to 12 months, based on the plan's monthly price.
- Prepayment is allowed, so a member may hold one active period plus future periods, but total coverage cannot exceed 12 months from the start of the first current or future period.
- Renewals may choose any available plan and are charged at that plan's current price, not a previously locked price.
- Payment is considered completed at the moment of sale; no external payment provider or refund flow is in scope.
- All payments use the plan's currency; multi-currency conversion is out of scope.
- Visit-limit enforcement is out of scope for this feature; the plan used by a period defines the limit, with no separate snapshot.
- Cancellation, refunds, and invoice documents are out of scope.
- The existing authentication and role model will later distinguish members from administrators; this feature assumes the caller is already identified.
