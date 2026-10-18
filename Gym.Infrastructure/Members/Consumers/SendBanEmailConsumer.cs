using Gym.Application.Common.Events.Members;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members.Consumers
{
    public class SendBanEmailConsumer : IConsumer<MemberBannedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;

        public SendBanEmailConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<MemberBannedIntegrationEvent> context)
        {
            var message = context.Message;

            var sb = new StringBuilder();
            sb.AppendLine($"Hello dear client {message.Name}");

            sb.AppendLine("You are banned by Admin of the Gym");

            sb.AppendLine("If you do not agree, send a rebuttal to this address.");

            string body = sb.ToString();

            await _emailSender.SendAsync(message.Email, "Banned by Admin", body);
        }
    }
}
