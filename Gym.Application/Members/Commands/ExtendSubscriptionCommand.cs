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

        public ExtendSubscriptionCommandHandler(IMemberRepository memberRepository , IUnitOfWork uow) { _memberRepository = memberRepository; _uow = uow; }


        public async Task Handle(ExtendSubscriptionCommand command , CancellationToken token = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.Id);

            if (member == null)
                throw new ArgumentNullException("That member not found");

            member.ExtendActiveSubscription(command.ExtraMonths);

            await _uow.SaveChangesAsync(token);
            

        }
    }



}
