using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;

namespace Gym.Domain.Members
{
    public sealed class InvalidMemberNameException : DomainException
    {
        public InvalidMemberNameException()
            : base(ErrorCodes.MemberNameInvalid, "Name must be filled.") { }
    }

    public sealed class InvalidSubscriptionDurationException : DomainException
    {
        public InvalidSubscriptionDurationException()
            : base(ErrorCodes.SubscriptionDurationInvalid, "Duration of subscription must be between 1 and 12 months.") { }
    }

    public sealed class MemberBannedException : DomainException
    {
        public MemberBannedException()
            : base(ErrorCodes.MemberBanned, "Banned members cannot perform this action.") { }
    }

    public sealed class SubscriptionCoverageExceededException : DomainException
    {
        public SubscriptionCoverageExceededException()
            : base(ErrorCodes.SubscriptionCoverageExceeded, "Total subscription coverage cannot exceed 12 months from the start of the first current or future period.") { }
    }

    public sealed class InvalidBodyMetricsException : DomainException
    {
        public InvalidBodyMetricsException()
            : base(ErrorCodes.MemberBodyMetricsInvalid, "Body metrics must be positive and goal must be filled.") { }
    }
}
