using FluentValidation;
using Gym.Application.Trainers.Commands;
using Gym.Application.Trainers.Queries;

namespace Gym.Application.Trainers.Validators
{
    public class CreateTrainerCommandValidator : AbstractValidator<CreateTrainerCommand>
    {
        public CreateTrainerCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
            RuleFor(x => x.SessionPrice).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }

    public class ChangeTrainerPriceCommandValidator : AbstractValidator<ChangeTrainerPriceCommand>
    {
        public ChangeTrainerPriceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.NewPrice).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }

    public class UpdateTrainerProfileCommandValidator : AbstractValidator<UpdateTrainerProfileCommand>
    {
        public UpdateTrainerProfileCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
        }
    }

    public class DeactivateTrainerCommandValidator : AbstractValidator<DeactivateTrainerCommand>
    {
        public DeactivateTrainerCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class GetTrainerProfileQueryValidator : AbstractValidator<GetTrainerProfileQuery>
    {
        public GetTrainerProfileQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class GetTrainerByIdAdminQueryValidator : AbstractValidator<GetTrainerByIdAdminQuery>
    {
        public GetTrainerByIdAdminQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class GetTrainerAvailabilityQueryValidator : AbstractValidator<GetTrainerAvailabilityQuery>
    {
        public GetTrainerAvailabilityQueryValidator()
        {
            RuleFor(x => x.TrainerId).NotEmpty();
            RuleFor(x => x.Date).NotEqual(default(DateOnly));
        }
    }

    public class GetActiveTrainersQueryValidator : AbstractValidator<GetActiveTrainersQuery>
    {
        public GetActiveTrainersQueryValidator()
        {
            RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    public class GetTrainersAdminQueryValidator : AbstractValidator<GetTrainersAdminQuery>
    {
        public GetTrainersAdminQueryValidator()
        {
            RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
