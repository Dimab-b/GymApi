using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }
}
