namespace Gym.Application.Payments.Dto_s
{
    public record RevenueDto(
        DateOnly From,
        DateOnly To,
        IReadOnlyList<RevenueTotalDto> Totals);

    public record RevenueTotalDto(
        string Currency,
        decimal Amount);
}
