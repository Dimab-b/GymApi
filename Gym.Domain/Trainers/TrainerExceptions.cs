using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;

namespace Gym.Domain.Trainers
{
    public sealed class InvalidTrainerNameException : DomainException
    {
        public InvalidTrainerNameException()
            : base(ErrorCodes.TrainerNameInvalid, "Trainer name cannot be empty.") { }
    }

    public sealed class InvalidSpecializationException : DomainException
    {
        public InvalidSpecializationException()
            : base(ErrorCodes.TrainerSpecializationInvalid, "Specialization must be filled.") { }
    }

    public sealed class TrainerInactiveException : DomainException
    {
        public TrainerInactiveException()
            : base(ErrorCodes.TrainerInactive, "Inactive trainer cannot change price or profile.") { }
    }

    public sealed class TrainerAlreadyInactiveException : DomainException
    {
        public TrainerAlreadyInactiveException()
            : base(ErrorCodes.TrainerAlreadyInactive, "Trainer is already inactive.") { }
    }
}
