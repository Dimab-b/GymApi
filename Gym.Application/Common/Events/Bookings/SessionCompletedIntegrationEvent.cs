using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Events.Bookings
{
    public record SessionCompletedIntegrationEvent(
        Guid BookingId,
        Guid MemberId,
        Guid TrainerId,
        DateTime EndTime);
}
