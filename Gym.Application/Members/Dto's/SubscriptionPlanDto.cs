using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Dto_s
{
    public record SubscriptionPlanDto(
        string Code,
        string Name,
        decimal PricePerMonth,
        string Currency,
        int? VisitLimitPerMonth);
}
