using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;

namespace Gym.Domain.Bookings
{
    public sealed class BookingTimeWindowException : DomainException
    {
        public BookingTimeWindowException(string message)
            : base(ErrorCodes.BookingTimeWindowInvalid, message) { }
    }

    public sealed class BookingAlreadyCompletedException : DomainException
    {
        public BookingAlreadyCompletedException()
            : base(ErrorCodes.BookingAlreadyCompleted, "You cannot change a session that has already been completed.") { }
    }

    public sealed class BookingCancellationTooLateException : DomainException
    {
        public BookingCancellationTooLateException()
            : base(ErrorCodes.BookingCancellationTooLate, "Session cannot be canceled less than 24 hours in advance.") { }
    }

    public sealed class BookingCannotCompleteException : DomainException
    {
        public BookingCannotCompleteException(string message)
            : base(ErrorCodes.BookingCannotComplete, message) { }
    }

    public sealed class BookingCannotCancelException : DomainException
    {
        public BookingCannotCancelException(string message)
            : base(ErrorCodes.BookingCannotCancel, message) { }
    }
}
