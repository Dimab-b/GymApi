using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Bookings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Events
{
    public class SessionCompletedDomainEventHandler : INotificationHandler<SessionCompletedDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public SessionCompletedDomainEventHandler(IIntegrationEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(SessionCompletedDomainEvent notification, CancellationToken cancellationToken)
            => _publisher.PublishAsync(new SessionCompletedIntegrationEvent(
                notification.Id,
                notification.MemberId,
                notification.TrainerId,
                notification.EndTime), cancellationToken);
    }
}
