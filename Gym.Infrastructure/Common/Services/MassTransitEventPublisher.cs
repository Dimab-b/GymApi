using Gym.Application.Common.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Common.Services
{
    public class MassTransitEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _endpoint;
        public MassTransitEventPublisher(IPublishEndpoint endpoint) => _endpoint = endpoint;
        public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
            => _endpoint.Publish(message, ct);
    }
}
