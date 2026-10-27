using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.SubscriptionPlans
{
    public record SubscriptionPlan(
        string Code,
        string Name,
        decimal PricePerMonth,
        string Currency,
        int? VisitLimitPerMonth);
}
