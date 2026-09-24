using Gym.Application.Common.Events.Members;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members.Consumers
{
    public class SendSubscriptionPurchasedEmailConsumer : IConsumer<SubscriptionPurchasedIntegrationEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemberRepository _memberRepository;
        public SendSubscriptionPurchasedEmailConsumer(IEmailSender emailSender , IMemberRepository memberRepository) 
        { 
            _emailSender = emailSender;
            _memberRepository = memberRepository;
        }

        public async Task Consume(ConsumeContext<SubscriptionPurchasedIntegrationEvent> context)
        {
            var member = await _memberRepository.GetByIdAsync(context.Message.MemberId , context.CancellationToken) ?? throw new ArgumentException("No member found");

            var sb = new StringBuilder();

            sb.AppendLine($"{member.Name} ,Thank for purchasing a subscription of our Gym");
            sb.AppendLine($"Start date of subscription: {context.Message.StartDate}");
            sb.AppendLine($"End date of subscription: {context.Message.EndDate}");
            sb.AppendLine("Enjoy your training");


            var body = sb.ToString();

            await _emailSender.SendAsync(member.Email.Value, "Purchased subscription", body);
        }
    }
}
