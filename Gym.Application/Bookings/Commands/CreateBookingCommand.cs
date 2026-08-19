using Gym.Domain.Bookings;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Commands
{
    public record CreateBookingCommand(Guid MemberId , Guid TrainerId , DateTime StartTime) : IRequest<Guid>;
    
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand , Guid>
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IBookingRepository _bookingRepository;

        public CreateBookingCommandHandler(ITrainerRepository trainerRepository , IBookingRepository bookingRepository)
        {
            _trainerRepository = trainerRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Guid> Handle(CreateBookingCommand command , CancellationToken cancellationToken = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(command.TrainerId, cancellationToken)
            ?? throw new KeyNotFoundException("Trainer not found.");

            if (!trainer.IsActive)
                throw new InvalidOperationException("Cannot book a session with an inactive trainer.");

            var endTime = command.StartTime.AddHours(1);
            var isOccupied = await _bookingRepository.HasOverlapAsync(command.TrainerId , command.StartTime , endTime);
            if (isOccupied)
                throw new InvalidOperationException("This time slot is already booked.");

            var booking = SessionBooking.Create(
                command.MemberId,
                command.TrainerId,
                command.StartTime,
                trainer.SessionPrice
                );

            _bookingRepository.Add(booking);

            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }
    }
}
