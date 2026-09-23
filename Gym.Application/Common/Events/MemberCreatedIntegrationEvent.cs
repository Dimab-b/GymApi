using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Events
{
    public record MemberCreatedIntegrationEvent(string Email);
}
