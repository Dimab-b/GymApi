namespace Gym.Domain.Common.Errors
{
    public static class ErrorCodes
    {
        public const string ValidationFailed = "validation.failed";
        public const string RequestInvalid = "request.invalid";
        public const string RequestMalformed = "request.malformed";
        public const string ResourceNotFound = "resource.not_found";
        public const string MemberNotFound = "member.not_found";
        public const string TrainerNotFound = "trainer.not_found";
        public const string BookingNotFound = "booking.not_found";
        public const string PaymentNotFound = "payment.not_found";
        public const string EmailDuplicate = "email.duplicate";
        public const string EmailInvalid = "email.invalid";
        public const string MemberBanned = "member.banned";
        public const string MemberNameInvalid = "member.name_invalid";
        public const string MemberBodyMetricsInvalid = "member.body_metrics_invalid";
        public const string SubscriptionDurationInvalid = "subscription.duration_invalid";
        public const string SubscriptionPlanUnavailable = "subscription.plan_unavailable";
        public const string SubscriptionCoverageExceeded = "subscription.coverage_exceeded";
        public const string SubscriptionPeriodConflict = "subscription.period_conflict";
        public const string SubscriptionRequired = "subscription.required";
        public const string PriceInvalid = "price.invalid";
        public const string CurrencyInvalid = "currency.invalid";
        public const string PaymentInvalid = "payment.invalid";
        public const string BookingTimeWindowInvalid = "booking.time_window_invalid";
        public const string BookingAlreadyCompleted = "booking.already_completed";
        public const string BookingCancellationTooLate = "booking.cancellation_too_late";
        public const string BookingCannotComplete = "booking.cannot_complete";
        public const string BookingCannotCancel = "booking.cannot_cancel";
        public const string BookingSlotTaken = "booking.slot_taken";
        public const string BookingNotOwned = "booking.not_owned";
        public const string TrainerInactive = "trainer.inactive";
        public const string TrainerAlreadyInactive = "trainer.already_inactive";
        public const string TrainerNameInvalid = "trainer.name_invalid";
        public const string TrainerSpecializationInvalid = "trainer.specialization_invalid";
        public const string AuthUnauthorized = "auth.unauthorized";
        public const string AuthForbidden = "auth.forbidden";
        public const string ConcurrencyConflict = "concurrency.conflict";
        public const string DataConflict = "data.conflict";
        public const string DataTransient = "data.transient";
        public const string IdempotencyConflict = "idempotency.conflict";
        public const string IdempotencyKeyRequired = "idempotency.key_required";
        public const string ServerError = "server.error";
        public const string RequestCancelled = "request.cancelled";
    }
}
