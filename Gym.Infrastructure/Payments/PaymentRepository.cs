using Gym.Domain.Payments;
using Gym.Infrastructure.Persistence;

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
    }
}
