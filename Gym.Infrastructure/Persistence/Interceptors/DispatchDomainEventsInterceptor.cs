using Gym.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gym.Infrastructure.Persistence.Interceptors
{
    public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;

        public DispatchDomainEventsInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
        {
            var domainEntities = context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            foreach (var entity in domainEntities)
            {
                entity.Entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
