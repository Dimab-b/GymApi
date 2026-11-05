namespace Gym.Domain.Members
{
    public sealed class SubscriptionConflictException : Exception
    {
        public SubscriptionConflictException(string message) : base(message) { }

        public SubscriptionConflictException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
