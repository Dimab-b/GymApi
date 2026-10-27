using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Bookings;
using Gym.Domain.Members;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Events
{
    public class SessionCancelledDomainEventHandler : INotificationHandler<SessionCancelledDomainEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;
        private readonly IMemberRepository _memberRepository;
        private readonly ITrainerRepository _trainerRepository;

        public SessionCancelledDomainEventHandler(
            IIntegrationEventPublisher publisher,
            IMemberRepository memberRepository,
            ITrainerRepository trainerRepository)
        {
            _publisher = publisher;
            _memberRepository = memberRepository;
            _trainerRepository = trainerRepository;
        }

        public async Task Handle(SessionCancelledDomainEvent notification, CancellationToken cancellationToken)
        {
            var member = await _memberRepository.GetByIdAsync(notification.MemberId, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            var trainer = await _trainerRepository.GetByIdAsync(notification.TrainerId, cancellationToken)
                ?? throw new KeyNotFoundException("Trainer not found.");

            await _publisher.PublishAsync(new SessionCancelledIntegrationEvent(
                notification.Id,
                notification.MemberId,
                member.Name,
                member.Email.Value,
                notification.TrainerId,
                trainer.Name,
                trainer.Email.Value,
                notification.StartTime,
                notification.Reason), cancellationToken);
        }
    }
}
