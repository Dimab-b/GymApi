using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Events.Bookings
{
    public record SessionScheduledIntegrationEvent(
        Guid BookingId,
        Guid MemberId,
        string MemberName,
        string MemberEmail,
        Guid TrainerId,
        string TrainerName,
        string TrainerEmail,
        DateTime StartTime,
        DateTime EndTime,
        decimal PriceAmount,
        string PriceCurrency);
}
