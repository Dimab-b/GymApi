namespace Gym.Application.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public string Code { get; }

        protected AppException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        protected AppException(string code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }
    }
}
