using FluentValidation;
using Gym.Application.Members.Commands;
using Gym.Application.Members.Queries;

namespace Gym.Application.Members.Validators
{
    public class PurchaseSubscriptionCommandValidator : AbstractValidator<PurchaseSubscriptionCommand>
    {
        public PurchaseSubscriptionCommandValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(50);
            RuleFor(x => x.DurationMonths).InclusiveBetween(1, 12);
            RuleFor(x => x.Method).IsInEnum();
            RuleFor(x => x.Purpose).IsInEnum();
            RuleFor(x => x.IdempotencyKey)
                .Must(key => !key.HasValue || key.Value != Guid.Empty)
                .WithMessage("Idempotency key must not be empty.");
        }
    }

    public class BanClientByIdCommandValidator : AbstractValidator<BanClientByIdCommand>
    {
        public BanClientByIdCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class RegisterMemberCommandValidator : AbstractValidator<RegisterMemberCommand>
    {
        public RegisterMemberCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        }
    }

    public class UpdateBodyMetricsCommandValidator : AbstractValidator<UpdateBodyMetricsCommand>
    {
        public UpdateBodyMetricsCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.height).GreaterThan(0);
            RuleFor(x => x.weight).GreaterThan(0);
            RuleFor(x => x.age).GreaterThan(0);
            RuleFor(x => x.goal).NotEmpty().MaximumLength(500);
        }
    }

    public class GetMemberByIdQueryValidator : AbstractValidator<GetMemberByIdQuery>
    {
        public GetMemberByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class GetMemberByIdWithSubscriptionsQueryValidator : AbstractValidator<GetMemberByIdWithSubscriptionsQuery>
    {
        public GetMemberByIdWithSubscriptionsQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
