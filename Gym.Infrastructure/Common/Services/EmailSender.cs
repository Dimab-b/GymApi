using Gym.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Common.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _from;
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly SecureSocketOptions _socketOptions;

        public EmailSender(IConfiguration configuration)
        {
            var settings = configuration.GetSection("EmailSettings");

            _from = settings["From"] ?? throw new InvalidOperationException("EmailSettings:From is not configured.");
            _host = settings["SmtpServer"] ?? throw new InvalidOperationException("EmailSettings:SmtpServer is not configured.");
            _port = int.TryParse(settings["Port"], out var port) ? port : 25;
            _username = settings["Username"] ?? throw new InvalidOperationException("EmailSettings:Username is not configured.");
            _password = settings["Password"] ?? throw new InvalidOperationException("EmailSettings:Password is not configured.");
            _socketOptions = Enum.TryParse<SecureSocketOptions>(settings["SecureSocketOptions"], true, out var socketOptions)
                ? socketOptions
                : SecureSocketOptions.StartTls;
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            await client.ConnectAsync(_host, _port, _socketOptions, cancellationToken);
            await client.AuthenticateAsync(_username, _password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
