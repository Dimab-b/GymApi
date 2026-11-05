using Gym.Domain.Common.Errors;

namespace Gym.Application.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string code, string message)
            : base(code, message) { }
    }

    public sealed class MemberNotFoundException : NotFoundException
    {
        public MemberNotFoundException(Guid id)
            : base(ErrorCodes.MemberNotFound, $"Member with ID {id} was not found.") { }
    }

    public sealed class TrainerNotFoundException : NotFoundException
    {
        public TrainerNotFoundException(Guid id)
            : base(ErrorCodes.TrainerNotFound, $"Trainer with ID {id} was not found.") { }
    }

    public sealed class BookingNotFoundException : NotFoundException
    {
        public BookingNotFoundException(Guid id)
            : base(ErrorCodes.BookingNotFound, $"Booking with ID {id} was not found.") { }
    }

    public sealed class PaymentNotFoundException : NotFoundException
    {
        public PaymentNotFoundException(Guid id)
            : base(ErrorCodes.PaymentNotFound, $"Payment with ID {id} was not found.") { }
    }

    public sealed class DuplicateEmailException : AppException
    {
        public DuplicateEmailException()
            : base(ErrorCodes.EmailDuplicate, "An account with this email already exists.") { }
    }

    public sealed class PlanUnavailableException : AppException
    {
        public PlanUnavailableException(string planCode)
            : base(ErrorCodes.SubscriptionPlanUnavailable, $"Plan '{planCode}' is not available.") { }
    }

    public sealed class SubscriptionRequiredException : AppException
    {
        public SubscriptionRequiredException()
            : base(ErrorCodes.SubscriptionRequired, "An active subscription is required to book a session.") { }
    }

    public sealed class BookingSlotTakenException : AppException
    {
        public BookingSlotTakenException()
            : base(ErrorCodes.BookingSlotTaken, "This time slot is already booked.") { }
    }

    public sealed class BookingNotOwnedException : AppException
    {
        public BookingNotOwnedException()
            : base(ErrorCodes.BookingNotOwned, "You cannot cancel another member's booking.") { }
    }

    public sealed class ForbiddenException : AppException
    {
        public ForbiddenException(string message)
            : base(ErrorCodes.AuthForbidden, message) { }
    }

    public sealed class ConcurrencyConflictException : AppException
    {
        public ConcurrencyConflictException(Exception innerException)
            : base(ErrorCodes.ConcurrencyConflict, "The data was modified by another operation. Please retry.", innerException) { }
    }

    public sealed class DataConflictException : AppException
    {
        public DataConflictException(Exception innerException)
            : base(ErrorCodes.DataConflict, "The operation conflicts with existing data.", innerException) { }
    }

    public sealed class TransientDataException : AppException
    {
        public TransientDataException(Exception innerException)
            : base(ErrorCodes.DataTransient, "A temporary data conflict occurred. Please retry.", innerException) { }
    }
}
