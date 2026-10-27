using Gym.Application.Common.Interfaces;
using Gym.Application.Common.SubscriptionPlans;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Gym.Infrastructure.Subscriptions
{
    public class SubscriptionPlanProvider : ISubscriptionPlanProvider
    {
        private readonly IReadOnlyList<SubscriptionPlan> _plans;

        public SubscriptionPlanProvider(IConfiguration configuration)
        {
            _plans = configuration.GetSection("SubscriptionPlans")
                .GetChildren()
                .Select(Map)
                .ToList();
        }

        public IReadOnlyList<SubscriptionPlan> GetAll() => _plans;

        public SubscriptionPlan? GetByCode(string code) =>
            _plans.FirstOrDefault(plan => string.Equals(plan.Code, code, StringComparison.OrdinalIgnoreCase));

        private static SubscriptionPlan Map(IConfigurationSection section)
        {
            var code = section["Code"] ?? throw new InvalidOperationException("SubscriptionPlans:Code is not configured.");
            var name = section["Name"] ?? throw new InvalidOperationException($"SubscriptionPlans:{code}:Name is not configured.");
            var currency = section["Currency"] ?? throw new InvalidOperationException($"SubscriptionPlans:{code}:Currency is not configured.");

            if (!decimal.TryParse(section["PricePerMonth"], NumberStyles.Number, CultureInfo.InvariantCulture, out var pricePerMonth) || pricePerMonth <= 0)
                throw new InvalidOperationException($"SubscriptionPlans:{code}:PricePerMonth must be a positive number.");

            int? visitLimitPerMonth = null;
            if (!string.IsNullOrWhiteSpace(section["VisitLimitPerMonth"]))
            {
                if (!int.TryParse(section["VisitLimitPerMonth"], out var parsedLimit) || parsedLimit < 1)
                    throw new InvalidOperationException($"SubscriptionPlans:{code}:VisitLimitPerMonth must be a positive integer.");
                visitLimitPerMonth = parsedLimit;
            }

            if (currency.Trim().Length != 3)
                throw new InvalidOperationException($"SubscriptionPlans:{code}:Currency must be 3 letters.");

            return new SubscriptionPlan(
                code,
                name,
                pricePerMonth,
                currency.Trim().ToUpperInvariant(),
                visitLimitPerMonth);
        }
    }
}
