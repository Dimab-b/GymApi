using Gym.Application.Common.Events;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Events
{
    public class MemberCreatedDomainEventHandler : INotificationHandler<MemberCreatedDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public MemberCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(MemberCreatedDomainEvent notification, CancellationToken cancellationToken = default) =>
             _publisher.PublishAsync(new MemberCreatedIntegrationEvent(notification.Email), cancellationToken);
    }
}
