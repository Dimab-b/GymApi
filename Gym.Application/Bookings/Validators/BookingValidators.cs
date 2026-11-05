using FluentValidation;
using Gym.Application.Bookings.Commands;
using Gym.Application.Bookings.Queries;
using Gym.Domain.Bookings;

namespace Gym.Application.Bookings.Validators
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.TrainerId).NotEmpty();
            RuleFor(x => x.StartTime).NotEqual(default(DateTime));
        }
    }

    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.BookingId).NotEmpty();
            RuleFor(x => x.Reason).MaximumLength(500);
        }
    }

    public class CompleteBookingCommandValidator : AbstractValidator<CompleteBookingCommand>
    {
        public CompleteBookingCommandValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty();
        }
    }

    public class GetMyBookingsQueryValidator : AbstractValidator<GetMyBookingsQuery>
    {
        public GetMyBookingsQueryValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<BookingStatus>(status, true, out _))
                .WithMessage("Status is invalid.");
        }
    }

    public class GetBookingsAdminQueryValidator : AbstractValidator<GetBookingsAdminQuery>
    {
        public GetBookingsAdminQueryValidator()
        {
            RuleFor(x => x.Page).InclusiveBetween(1, 100000);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
            RuleFor(x => x.To)
                .Must((query, to) => !query.From.HasValue || !to.HasValue || query.From <= to)
                .WithMessage("The start date must be earlier than or equal to the end date.");
        }
    }
}
