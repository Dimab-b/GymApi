namespace Gym.Application.Payments.Dto_s
{
    public record RevenueDto(
        DateOnly From,
        DateOnly To,
        decimal TotalAmount,
        string? Currency);
}
