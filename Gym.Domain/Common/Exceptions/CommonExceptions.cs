using Gym.Domain.Common.Errors;

namespace Gym.Domain.Common.Exceptions
{
    public sealed class InvalidEmailException : DomainException
    {
        public InvalidEmailException()
            : base(ErrorCodes.EmailInvalid, "Invalid email format.") { }
    }

    public sealed class InvalidPriceException : DomainException
    {
        public InvalidPriceException()
            : base(ErrorCodes.PriceInvalid, "Value must be greater than 0.") { }
    }

    public sealed class InvalidCurrencyException : DomainException
    {
        public InvalidCurrencyException()
            : base(ErrorCodes.CurrencyInvalid, "Currency must be in 3 letters.") { }
    }
}
