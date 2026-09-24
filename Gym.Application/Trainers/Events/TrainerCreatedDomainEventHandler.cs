using Gym.Application.Common.Events.Trainers;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Events
{
    public class TrainerCreatedDomainEventHandler : INotificationHandler<TrainerCreatedDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public TrainerCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
        {
            _publisher = publisher;
        }
        public Task Handle(TrainerCreatedDomainEvent notification , CancellationToken cancellationToken = default) =>
            _publisher.PublishAsync(new TrainerCreatedIntegrationEvent(notification.Name , notification.Email), cancellationToken);
    }
}
