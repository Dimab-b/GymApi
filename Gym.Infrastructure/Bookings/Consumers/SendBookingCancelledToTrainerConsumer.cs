using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Bookings.Consumers
{
    public class SendBookingCancelledToTrainerConsumer : IConsumer<SessionCancelledIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendBookingCancelledToTrainerConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<SessionCancelledIntegrationEvent> context)
        {
            var message = context.Message;

            var body = new StringBuilder()
                .AppendLine($"Hello {message.TrainerName},")
                .AppendLine($"The session with {message.MemberName} at {message.StartTime:u} has been cancelled.")
                .AppendLine($"Reason: {message.Reason}")
                .ToString();

            await _emailSender.SendAsync(message.TrainerEmail, "Booking cancelled", body, context.CancellationToken);
        }
    }
}
