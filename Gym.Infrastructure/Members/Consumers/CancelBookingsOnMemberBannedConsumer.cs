using Gym.Application.Common.Events.Members;
using Gym.Domain.Bookings;
using Gym.Domain.Common;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members.Consumers
{
    public class CancelBookingsOnMemberBannedConsumer : IConsumer<MemberBannedIntegrationEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;

        public CancelBookingsOnMemberBannedConsumer(IBookingRepository bookingRepository, IUnitOfWork uow)
        {
            _bookingRepository = bookingRepository;
            _uow = uow;
        }

        public async Task Consume(ConsumeContext<MemberBannedIntegrationEvent> context)
        {
            var bookings = await _bookingRepository.GetScheduledByMemberIdAsync(
                context.Message.MemberId,
                context.CancellationToken);

            foreach (var booking in bookings)
                booking.CancelBySystem("Member banned");

            await _uow.SaveChangesAsync(context.CancellationToken);
        }
    }
}
