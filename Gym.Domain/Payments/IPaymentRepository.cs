namespace Gym.Domain.Payments
{
    public interface IPaymentRepository
    {
        void Add(Payment payment);

        Task<Payment?> GetByIdempotencyKeyAsync(
            Guid memberId,
            Guid idempotencyKey,
            CancellationToken cancellationToken = default);
    }
}
