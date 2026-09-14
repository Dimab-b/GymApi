using Gym.Domain.Bookings;
using Gym.Domain.Common;
using Gym.Domain.Members;
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
        private readonly IMemberRepository _memberRepository;
        private readonly ITrainerRepository _trainerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;

        public CreateBookingCommandHandler(IMemberRepository memberRepository, ITrainerRepository trainerRepository , IBookingRepository bookingRepository , IUnitOfWork uow)
        {
            _memberRepository = memberRepository;
            _trainerRepository = trainerRepository;
            _bookingRepository = bookingRepository;
            _uow = uow;
        }

        public async Task<Guid> Handle(CreateBookingCommand command , CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.MemberId , cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            var trainer = await _trainerRepository.GetByIdAsync(command.TrainerId, cancellationToken)
                ?? throw new KeyNotFoundException("Trainer not found.");

            if (member.IsBanned)
                throw new InvalidOperationException("Banned members cannot book sessions.");

            if (!member.HasActiveSubscription())
                throw new InvalidOperationException("An active subscription is required to book a session.");

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

            await _uow.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }
    }
}
