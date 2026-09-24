using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Events.Members
{
    public record SubscriptionPurchasedIntegrationEvent(Guid MemberId, Guid SubscriptionId, DateTime StartDate, DateTime EndDate);
}
