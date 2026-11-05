using FluentValidation;
using Gym.Application.Payments.Queries;

namespace Gym.Application.Payments.Validators
{
    public class GetMemberPaymentsQueryValidator : AbstractValidator<GetMemberPaymentsQuery>
    {
        public GetMemberPaymentsQueryValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    public class GetRevenueQueryValidator : AbstractValidator<GetRevenueQuery>
    {
        public GetRevenueQueryValidator()
        {
            RuleFor(x => x.To)
                .Must((query, to) => query.From <= to)
                .WithMessage("The start date must be earlier than or equal to the end date.");

            RuleFor(x => x.To)
                .Must(to => to != DateOnly.MaxValue)
                .WithMessage("The end date is out of the supported range.");
        }
    }
}
