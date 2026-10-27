using Gym.Application.Common.Events.Members;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members.Consumers
{
    public class SendSubscriptionPurchasedEmailConsumer : IConsumer<SubscriptionPurchasedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendSubscriptionPurchasedEmailConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<SubscriptionPurchasedIntegrationEvent> context)
        {
            var message = context.Message;

            var sb = new StringBuilder();

            sb.AppendLine($"{message.Name} ,Thank for purchasing a subscription of our Gym");
            sb.AppendLine($"Start date of subscription: {message.StartDate}");
            sb.AppendLine($"End date of subscription: {message.EndDate}");
            sb.AppendLine("Enjoy your training");


            var body = sb.ToString();

            await _emailSender.SendAsync(message.Email, "Purchased subscription", body, context.CancellationToken);
        }
    }
}
