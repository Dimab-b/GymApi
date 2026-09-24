using Gym.Application.Common.Events.Trainers;
using Gym.Domain.Bookings;
using Gym.Domain.Common;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Trainers.Consumers
{
    public class CancelBookingsOnTrainerDeactivatedConsumer : IConsumer<TrainerDeactivatedIntegrationEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;
        public CancelBookingsOnTrainerDeactivatedConsumer(IBookingRepository bookingRepository , IUnitOfWork uow) { _bookingRepository = bookingRepository; _uow = uow; }
        public async Task Consume(ConsumeContext<TrainerDeactivatedIntegrationEvent> context)
        {
            var sessions = await _bookingRepository.GetByTrainerId(context.Message.TrainerId, context.CancellationToken);
            foreach (var session in sessions)
            {
                session.CancelBySystem();
            }
            await _uow.SaveChangesAsync(context.CancellationToken);
        }
    }
}
