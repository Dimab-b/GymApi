using Gym.Domain.Bookings;
using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Commands
{
    public record CancelBookingCommand(Guid MemberId , Guid BookingId) : IRequest;
    
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;
        public CancelBookingCommandHandler(IBookingRepository bookingRepository , IUnitOfWork uow)
        {
            _bookingRepository = bookingRepository;
            _uow = uow;
        }

        public async Task Handle(CancelBookingCommand command , CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.MemberId != command.MemberId)
                throw new InvalidOperationException("You cannot cancel another member's booking.");

            booking.Cancel();

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
