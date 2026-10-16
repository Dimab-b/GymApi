using Gym.Domain.Bookings;
using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Commands
{
    public record CompleteBookingCommand(Guid BookingId) : IRequest;

    public class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;

        public CompleteBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork uow)
        {
            _bookingRepository = bookingRepository;
            _uow = uow;
        }

        public async Task Handle(CompleteBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
                ?? throw new KeyNotFoundException("Booking not found.");

            booking.Complete();

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
