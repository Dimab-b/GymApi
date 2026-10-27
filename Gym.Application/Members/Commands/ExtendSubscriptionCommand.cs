using Gym.Application.Common.Interfaces;
using Gym.Domain.Common;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record ExtendSubscriptionCommand(Guid Id , int ExtraMonths) : IRequest;

    public class ExtendSubscriptionCommandHandler : IRequestHandler<ExtendSubscriptionCommand>
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _uow;
        private readonly ISubscriptionPlanProvider _planProvider;

        public ExtendSubscriptionCommandHandler(
            IMemberRepository memberRepository,
            IUnitOfWork uow,
            ISubscriptionPlanProvider planProvider)
        {
            _memberRepository = memberRepository;
            _uow = uow;
            _planProvider = planProvider;
        }

        public async Task Handle(ExtendSubscriptionCommand command , CancellationToken token = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.Id , token);

            if (member == null)
                throw new KeyNotFoundException("That member not found");

            var activeSubscription = member.ActiveSubscription
                ?? throw new InvalidOperationException("Member does not have an active subscription to extend.");

            var plan = _planProvider.GetByCode(activeSubscription.PlanCode)
                ?? throw new InvalidOperationException($"Plan '{activeSubscription.PlanCode}' is no longer available.");

            member.PurchaseSubscription(
                plan.Code,
                plan.PricePerMonth,
                plan.Currency,
                command.ExtraMonths,
                plan.VisitLimitPerMonth);

            await _uow.SaveChangesAsync(token);
        }
    }



}
