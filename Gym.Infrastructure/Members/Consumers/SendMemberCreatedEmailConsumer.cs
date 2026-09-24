using Gym.Application.Common.Events.Members;
using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gym.Infrastructure.Members.Consumers
{
    public class SendMemberCreatedEmailConsumer : IConsumer<MemberCreatedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;
        public SendMemberCreatedEmailConsumer (IEmailSender emailSender) => _emailSender = emailSender;

        public async Task Consume(ConsumeContext<MemberCreatedIntegrationEvent> context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Congratulations with creating account of our gym");
            sb.AppendLine("We look forward to long-term cooperation");
            string body = sb.ToString();
            await _emailSender.SendAsync(context.Message.Email, "Your account is created", body);
        }
    }
}
