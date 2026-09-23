using Gym.Application.Common.Events;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members.Consumers
{
    public class SendBanEmailConsumer : IConsumer<MemberBannedEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemberRepository _memberRepository;

        public SendBanEmailConsumer(IEmailSender emailSender, IMemberRepository memberRepository)
        {
            _emailSender = emailSender;
            _memberRepository = memberRepository;
        }

        public async Task Consume(ConsumeContext<MemberBannedEvent> context)
        {
            var member = await _memberRepository.GetByIdAsync(context.Message.MemberId, context.CancellationToken)
                ?? throw new ArgumentException("No member found");

            var sb = new StringBuilder();
            sb.AppendLine($"Hello dear client {member.Name}");

            sb.AppendLine("You are banned by Admin of the Gym");

            sb.AppendLine("If you do not agree, send a rebuttal to this address.");

            string body = sb.ToString();

            await _emailSender.SendAsync(member.Email.Value, "Banned by Admin", body);
        }
    }
}
