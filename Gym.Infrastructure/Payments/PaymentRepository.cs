using Gym.Domain.Payments;
using Gym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Payments
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Payment payment)
        {
            _context.Payments.Add(payment);
        }

        public async Task<Payment?> GetByIdempotencyKeyAsync(
            Guid memberId,
            Guid idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            _context.ChangeTracker.Clear();

            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    payment => payment.MemberId == memberId && payment.IdempotencyKey == idempotencyKey,
                    cancellationToken);
        }
    }
}
