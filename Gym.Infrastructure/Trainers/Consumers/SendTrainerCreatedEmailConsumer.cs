using Gym.Application.Common.Events.Trainers;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Trainers.Consumers
{
    public class SendTrainerCreatedEmailConsumer : IConsumer<TrainerCreatedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendTrainerCreatedEmailConsumer(IEmailSender emailSender) => _emailSender = emailSender;

        public async Task Consume(ConsumeContext<TrainerCreatedIntegrationEvent> context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Congratulations with joining to our gym");
            sb.AppendLine("We look forward to long-term cooperation");
            string body = sb.ToString();
            await _emailSender.SendAsync(context.Message.Email, $"{context.Message.Name} , You You can acquire clients.", body);
        }
    }
}
