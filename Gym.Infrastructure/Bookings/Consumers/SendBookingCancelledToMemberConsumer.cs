using Gym.Application.Common.Events.Bookings;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Bookings.Consumers
{
    public class SendBookingCancelledToMemberConsumer : IConsumer<SessionCancelledIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendBookingCancelledToMemberConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<SessionCancelledIntegrationEvent> context)
        {
            var message = context.Message;

            var body = new StringBuilder()
                .AppendLine($"Hello {message.MemberName},")
                .AppendLine($"Your session with {message.TrainerName} at {message.StartTime:u} has been cancelled.")
                .AppendLine($"Reason: {message.Reason}")
                .ToString();

            await _emailSender.SendAsync(message.MemberEmail, "Booking cancelled", body);
        }
    }
}
