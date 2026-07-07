using Gym.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Bookings
{
    public record SessionScheduledEvent(Guid Id , Guid MemberId ,Guid TrainerId, DateTime StartTime) : IDomainEvent;
    public record SessionCancelledEvent(Guid Id  , Guid MemberId , Guid TrainerId): IDomainEvent;
    
    
}
