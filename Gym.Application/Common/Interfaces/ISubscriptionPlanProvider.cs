using Gym.Application.Common.SubscriptionPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Interfaces
{
    public interface ISubscriptionPlanProvider
    {
        IReadOnlyList<SubscriptionPlan> GetAll();
        SubscriptionPlan? GetByCode(string code);
    }
}
