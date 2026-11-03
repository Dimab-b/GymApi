namespace Gym.Application.Payments.Dto_s
{
    public record PaymentDto(
        Guid Id,
        Guid MemberId,
        Guid? SubscriptionId,
        decimal Amount,
        string Currency,
        DateTime PaidAt,
        string Method,
        string Purpose,
        string Status,
        string? Note);
}
