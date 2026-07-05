using FluentEmail.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Common.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailSender(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            await _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(body)
                .SendAsync();
        }
    }
}
