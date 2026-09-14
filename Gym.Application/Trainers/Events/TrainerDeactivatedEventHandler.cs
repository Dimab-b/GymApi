using Gym.Domain.Bookings;
using Gym.Domain.Common;
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
        private readonly IUnitOfWork _uow;

        public TrainerDeactivatedEventHandler(IBookingRepository bookingRepository, IUnitOfWork uow)
        {
            _bookingRepository = bookingRepository;
            _uow = uow;
        }
        public async Task Handle(TrainerDeactivatedEvent TrainerEvent , CancellationToken cancellationToken = default)
        {
            var sessions = await _bookingRepository.GetByTrainerId(TrainerEvent.TrainerId, cancellationToken);
            foreach(var session in sessions)
            {
                session.CancelBySystem();
            }
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
