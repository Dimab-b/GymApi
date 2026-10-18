using Gym.Domain.Bookings;
using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Commands
{
    public record CompleteExpiredBookingsCommand : IRequest<int>;

    public class CompleteExpiredBookingsCommandHandler : IRequestHandler<CompleteExpiredBookingsCommand, int>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;

        public CompleteExpiredBookingsCommandHandler(IBookingRepository bookingRepository, IUnitOfWork uow)
        {
            _bookingRepository = bookingRepository;
            _uow = uow;
        }

        public async Task<int> Handle(CompleteExpiredBookingsCommand command, CancellationToken cancellationToken)
        {
            var expired = await _bookingRepository.GetExpiredScheduledAsync(DateTime.UtcNow, cancellationToken);

            foreach (var booking in expired)
                booking.Complete();

            await _uow.SaveChangesAsync(cancellationToken);

            return expired.Count;
        }
    }
}
