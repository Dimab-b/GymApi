namespace Gym.Application.Members.Dto_s
{
    public record PurchaseResultDto(
        Guid SubscriptionId,
        decimal Amount,
        string Currency);
}
