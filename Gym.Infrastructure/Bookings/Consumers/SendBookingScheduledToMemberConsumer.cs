using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Bookings.Consumers
{
    public class SendBookingScheduledToMemberConsumer : IConsumer<SessionScheduledIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendBookingScheduledToMemberConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<SessionScheduledIntegrationEvent> context)
        {
            var message = context.Message;

            var body = new StringBuilder()
                .AppendLine($"Hello {message.MemberName},")
                .AppendLine($"Your session with {message.TrainerName} is confirmed.")
                .AppendLine($"When: {message.StartTime:u} - {message.EndTime:u}")
                .AppendLine($"Price: {message.PriceAmount} {message.PriceCurrency}")
                .ToString();

            await _emailSender.SendAsync(message.MemberEmail, "Booking confirmed", body, context.CancellationToken);
        }
    }
}
