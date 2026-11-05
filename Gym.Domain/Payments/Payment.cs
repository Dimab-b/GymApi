using Gym.Domain.Common;
using Gym.Domain.Common.VO;

namespace Gym.Domain.Payments
{
    public class Payment : Entity, IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid MemberId { get; private set; }
        public Guid? SubscriptionId { get; private set; }
        public Price Amount { get; private set; } = null!;
        public DateTime PaidAt { get; private set; }
        public PaymentMethod Method { get; private set; }
        public PaymentPurpose Purpose { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? Note { get; private set; }

        private Payment() { }

        public static Payment Create(
            Guid memberId,
            Guid? subscriptionId,
            Price amount,
            PaymentMethod method,
            PaymentPurpose purpose,
            string? note = null)
        {
            if (memberId == Guid.Empty)
                throw new InvalidPaymentException("Member id must be filled.");

            if (subscriptionId == Guid.Empty)
                throw new InvalidPaymentException("Subscription id must be a non-empty value when provided.");

            ArgumentNullException.ThrowIfNull(amount);

            return new Payment
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                SubscriptionId = subscriptionId,
                Amount = amount,
                PaidAt = DateTime.UtcNow,
                Method = method,
                Purpose = purpose,
                Status = PaymentStatus.Completed,
                Note = note
            };
        }
    }
}
