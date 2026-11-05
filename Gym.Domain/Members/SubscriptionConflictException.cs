using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;

namespace Gym.Domain.Members
{
    public sealed class SubscriptionConflictException : DomainException
    {
        public SubscriptionConflictException(string message)
            : base(ErrorCodes.SubscriptionPeriodConflict, message) { }

        public SubscriptionConflictException(string message, Exception innerException)
            : base(ErrorCodes.SubscriptionPeriodConflict, message, innerException) { }
    }
}
