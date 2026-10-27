using Gym.Application.Common.Interfaces;
using Gym.Application.Members.Dto_s;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gym.Application.Members.Queries
{
    public record GetSubscriptionPlansQuery : IRequest<IReadOnlyList<SubscriptionPlanDto>>;

    public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
    {
        private readonly ISubscriptionPlanProvider _planProvider;

        public GetSubscriptionPlansQueryHandler(ISubscriptionPlanProvider planProvider)
        {
            _planProvider = planProvider;
        }

        public Task<IReadOnlyList<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<SubscriptionPlanDto> plans = _planProvider.GetAll()
                .Select(plan => new SubscriptionPlanDto(
                    plan.Code,
                    plan.Name,
                    plan.PricePerMonth,
                    plan.Currency,
                    plan.VisitLimitPerMonth))
                .ToList();

            return Task.FromResult(plans);
        }
    }
}
