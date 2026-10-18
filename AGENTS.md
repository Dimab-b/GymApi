
# Project Instructions

## General workflow
- Inspect the relevant code before making changes.
- For non-trivial tasks, analyze first and propose a plan.
- Implement only the requested scope.
- Do not refactor unrelated code.
- Explain important changes and list modified files.
- Never claim a build or test passed unless it was run.

## Architecture
- Follow the existing architecture and dependency direction.
- Keep business rules out of API controllers.
- Keep Domain independent from Infrastructure.
- Put use cases in the existing Application layer.
- Follow existing CQRS and MediatR conventions.
- Do not introduce new projects, patterns, or abstractions
  without a concrete reason.

## C# and ASP.NET Core
- Prefer clear, maintainable code over clever abstractions.
- Use async/await for asynchronous I/O.
- Propagate CancellationToken where appropriate.
- Use dependency injection consistently.
- Validate external input at the application boundary.
- Preserve existing API contracts unless the task requires changes.
- Do not swallow exceptions.
- Never log passwords, tokens, or secrets.

## EF Core and PostgreSQL
- Inspect entity configurations and migrations before editing.
- Avoid unnecessary database queries and N+1 problems.
- Consider transactions and concurrent updates.
- Never drop, reset, or delete database data without approval.
- Do not silently rewrite migrations that may already be applied.

## Tests and verification
- Inspect existing tests before adding new ones.
- Add tests for meaningful behavior changes.
- Cover important failure cases and edge cases.
- Run the relevant test project after implementation.
- Report which commands were executed and their actual results.

## Git safety
- Inspect git status before modifying files.
- Do not overwrite unrelated user changes.
- Do not run destructive Git commands without approval.
- Do not commit or push unless explicitly requested.



## Security and configuration
- Keep secrets (DB, SMTP, RabbitMQ) in User Secrets locally; use `.env`
  (git-ignored) for Docker Compose. Never commit `.env` or any secret.
- Keep MassTransit on 8.x (9.x requires a license). Keep Hangfire on 1.8.x.
- Never log passwords, tokens, connection strings, or credentials.

## CQRS conventions
- A command/query is a `record`; its handler lives in the same file.
- Naming: `XxxCommand`/`XxxQuery` + `XxxCommandHandler`/`XxxQueryHandler`.
- Commands return `Guid`/`bool`/`int` or `void`; queries return a DTO or
  `PagedResult<T>`.
- One write = one aggregate. Aggregates reference each other by `Id`,
  never by object references.
- Cross-aggregate coordination goes through domain events, not by calling
  other repositories directly.

## Domain and integration events
- Domain event: `...DomainEvent : IDomainEvent` in
  `Gym.Domain/<Module>/<Module>Events.cs`; raised only by the aggregate
  via `AddDomainEvent`.
- Do not publish integration events from command handlers. Chain:
  `DomainEvent` -> bridge `INotificationHandler<...DomainEvent>`
  (`Application/<Module>/Events`) -> `...IntegrationEvent`
  (`Application/Common/Events/<Module>`) -> `IConsumer<...IntegrationEvent>`
  (`Infrastructure/<Module>/Consumers`).
- The bridge only translates (maps and `PublishAsync`) - no side effects.
  Side effects (email, etc.) belong to consumers only.
- Consumer class names must be unique within the assembly (the queue name is
  the class name without namespace; duplicates share an endpoint and create
  `_skipped` queues).
- Use `IIntegrationEventPublisher`/`IPublishEndpoint` for integration events,
  never `IBus`.
- State changes go through the Bus Outbox; consumers use the Consumer
  Outbox/Inbox. Never publish directly to the broker.
- `...IntegrationEvent` is a stable contract; the domain may change, the
  contract only carefully.

## Domain events and EF Core
- `DispatchDomainEventsInterceptor` runs on `SavingChangesAsync` (before
  commit) so the outbox write is in the same transaction. Do not move it to
  `SavedChangesAsync` while the bus outbox is enabled.
- `ExecuteUpdate`/`ExecuteDelete` bypass domain events. For aggregate changes
  that must raise events, load the entity + call the domain method +
  `SaveChanges`. Use bulk operations only for purely technical/idempotent work.