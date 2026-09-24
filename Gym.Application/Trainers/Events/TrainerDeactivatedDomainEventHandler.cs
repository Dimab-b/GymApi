using Gym.Application.Common.Events.Trainers;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Bookings;
using Gym.Domain.Common;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gym.Application.Trainers.Events
{
    public class TrainerDeactivatedDomainEventHandler : INotificationHandler<TrainerDeactivatedDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public TrainerDeactivatedDomainEventHandler(IIntegrationEventPublisher publisher) => _publisher = publisher;
        public Task Handle(TrainerDeactivatedDomainEvent notification, CancellationToken cancellationToken = default) =>
            _publisher.PublishAsync(new TrainerDeactivatedIntegrationEvent(notification.TrainerId) , cancellationToken);
    }
}
