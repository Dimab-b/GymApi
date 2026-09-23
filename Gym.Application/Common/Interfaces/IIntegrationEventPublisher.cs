using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Interfaces
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    }
}
