using Gym.Domain.Bookings;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Events
{
    public class TrainerDeactivatedEventHandler : INotificationHandler<TrainerDeactivatedEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        public TrainerDeactivatedEventHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }
        public async Task Handle(TrainerDeactivatedEvent TrainerEvent , CancellationToken cancellationToken = default)
        {
            var sessions = await _bookingRepository.GetByTrainerId(TrainerEvent.TrainerId, cancellationToken);
            foreach(var session in sessions)
            {
                session.CancelBySystem();
            }
            await _bookingRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
