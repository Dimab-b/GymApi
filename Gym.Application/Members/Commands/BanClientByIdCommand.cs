using Gym.Application.Common.Events;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record BanClientByIdCommand(Guid Id) : IRequest<bool>;
    
    public class BanClientByIdCommandHandler : IRequestHandler<BanClientByIdCommand , bool>
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _uow;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public BanClientByIdCommandHandler(IMemberRepository memberRepository , IUnitOfWork uow , IIntegrationEventPublisher integrationEventPublisher)
        {
            _memberRepository = memberRepository;
            _uow = uow;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<bool> Handle(BanClientByIdCommand command , CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.Id, cancellationToken);

            if (member == null)
            {
                throw new KeyNotFoundException("That member not found");
            }


            member.BanUser();

            await _uow.SaveChangesAsync(cancellationToken);

            await _integrationEventPublisher.PublishAsync(new MemberBannedEvent(member.Id) , cancellationToken);

            return true;
        }
    }
}
