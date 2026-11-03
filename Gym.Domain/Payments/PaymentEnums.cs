namespace Gym.Domain.Payments
{
    public enum PaymentMethod
    {
        Cash,
        Terminal,
        Online
    }

    public enum PaymentPurpose
    {
        NewSubscription,
        Extension
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Refunded
    }
}
