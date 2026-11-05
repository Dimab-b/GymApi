using Microsoft.Extensions.Configuration;

namespace Gym.Application.Common.Time
{
    public sealed class GymTimeZone
    {
        public TimeZoneInfo Value { get; }

        public GymTimeZone(IConfiguration configuration)
        {
            var configuredId = configuration["Gym:TimeZone"];
            Value = Resolve(string.IsNullOrWhiteSpace(configuredId) ? "Europe/Kyiv" : configuredId);
        }

        public DateTime ToUtc(DateOnly localDate)
        {
            var localDateTime = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, Value);
        }

        public DateTime ToUtc(DateTime localDateTime)
        {
            var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, Value);
        }

        public TimeSpan ToLocalTimeOfDay(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, Value).TimeOfDay;
        }

        private static TimeZoneInfo Resolve(string id)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    throw new InvalidOperationException($"Time zone '{id}' is not available on this host.");
                }
            }
            catch (InvalidTimeZoneException exception)
            {
                throw new InvalidOperationException($"Time zone '{id}' is invalid.", exception);
            }
        }
    }
}
