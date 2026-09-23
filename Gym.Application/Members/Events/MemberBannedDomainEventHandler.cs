using Gym.Application.Common.Events;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Gym.Application.Members.Events
{
    public class MemberBannedDomainEventHandler : INotificationHandler<MemberBannedDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public MemberBannedDomainEventHandler(IIntegrationEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(MemberBannedDomainEvent notification, CancellationToken cancellationToken)
            => _publisher.PublishAsync(new MemberBannedIntegrationEvent(notification.MemberId), cancellationToken);
    }
}
