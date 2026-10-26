using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Bookings.Consumers
{
    public class SendBookingScheduledToTrainerConsumer : IConsumer<SessionScheduledIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendBookingScheduledToTrainerConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<SessionScheduledIntegrationEvent> context)
        {
            var message = context.Message;

            var body = new StringBuilder()
                .AppendLine($"Hello {message.TrainerName},")
                .AppendLine($"You have a new session booked by {message.MemberName}.")
                .AppendLine($"When: {message.StartTime:u} - {message.EndTime:u}")
                .AppendLine($"Price: {message.PriceAmount} {message.PriceCurrency}")
                .ToString();

            await _emailSender.SendAsync(message.TrainerEmail, "New booking", body, context.CancellationToken);
        }
    }
}
