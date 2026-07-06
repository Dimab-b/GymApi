using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Events
{
    public class MemberBannedEventHandler : INotificationHandler<MemberBannedEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemberRepository _memberRepository;

        public MemberBannedEventHandler(IEmailSender emailSender , IMemberRepository memberRepository)
        {
            _emailSender = emailSender;
            _memberRepository = memberRepository;
        }

        public async Task Handle(MemberBannedEvent bannedEvent , CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(bannedEvent.MemberId) ?? throw new ArgumentException("No member found");

            var sb = new StringBuilder();
            sb.AppendLine($"Hello dear client {member.Name}");

            sb.AppendLine("You are banned by Admin of the Gym");

            sb.AppendLine("If you do not agree, send a rebuttal to this address.");

            string body = sb.ToString();

            await _emailSender.SendAsync(member.Email.Value, "Banned by Admin", body);
        }
    }
}
