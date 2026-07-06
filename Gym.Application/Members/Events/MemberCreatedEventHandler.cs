using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Events
{
    public class MemberCreatedEventHandler : INotificationHandler<MemberCreatedEvent>
    {
        private readonly IEmailSender _emailSender;
        public MemberCreatedEventHandler(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Handle(MemberCreatedEvent command , CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Congratulations with creating account of our gym");
            sb.AppendLine("We look forward to long-term cooperation");
            string body = sb.ToString();
            await _emailSender.SendAsync(command.Email , "Your account is created" , body);
        }


    }
}
