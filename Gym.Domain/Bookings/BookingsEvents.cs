using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Bookings
{
    public record SessionScheduledDomainEvent(
        Guid Id,
        Guid MemberId,
        Guid TrainerId,
        DateTime StartTime,
        DateTime EndTime,
        Price Price) : IDomainEvent;

    public record SessionCancelledDomainEvent(
        Guid Id,
        Guid MemberId,
        Guid TrainerId,
        DateTime StartTime,
        string Reason) : IDomainEvent;

    public record SessionCompletedDomainEvent(
        Guid Id,
        Guid MemberId,
        Guid TrainerId,
        DateTime EndTime) : IDomainEvent;
}
