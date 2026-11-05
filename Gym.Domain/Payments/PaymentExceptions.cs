using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;

namespace Gym.Domain.Payments
{
    public sealed class InvalidPaymentException : DomainException
    {
        public InvalidPaymentException(string message)
            : base(ErrorCodes.PaymentInvalid, message) { }
    }
}
