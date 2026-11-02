<!--
Sync Impact Report
- Version change: 1.0.0 -> 1.1.0
- Modified principles: none
- Modified sections: Development Workflow and Quality Gates (added an explicit
  clarify step to the Spec Kit flow: specify -> clarify -> plan -> tasks -> implement)
- Added sections: none
- Removed sections: none
- Follow-up TODOs: none
-->

# GymAndTrainingApi Constitution

## Core Principles

### I. Layered Architecture and Dependency Direction

The solution is a four-project Clean Architecture: `Gym.Domain`,
`Gym.Application`, `Gym.Infrastructure`, and `GymAndTrainingApi` (`Gym.Api`).
Dependencies MUST flow `Api -> Infrastructure -> Application -> Domain`; the
Domain MUST NOT reference Infrastructure or Application. Business rules MUST NOT
live in API controllers - controllers only send MediatR requests. Use cases
belong in the Application layer. Do not introduce new projects, patterns, or
abstractions without a concrete reason. Preserve existing folder/namespace
quirks (`Dto's`/`Dto_s`, `Value Objects`/`Value_Objects`, project
`GymAndTrainingApi` with namespace `Gym.Api`).

Before writing new code, search the repository for an existing equivalent
pattern (command, query, handler, consumer, bridge, repository, or query) and
extend or reuse it. MUST NOT duplicate domain or application logic.

### II. CQRS and Aggregate Boundaries

A command or query is a `record`; its handler lives in the same file. Naming:
`XxxCommand`/`XxxQuery` plus `XxxCommandHandler`/`XxxQueryHandler`. Commands
return `Guid`/`bool`/`int` or `void`; queries return a DTO or `PagedResult<T>`.
One write equals one aggregate. Aggregates reference other aggregates by `Id`,
never by object references. Cross-aggregate coordination goes through domain
events, not by calling other repositories directly. The read side uses Dapper
with DTOs: all values MUST be parameterized, nullable optional filters require
explicit casts, and pagination MUST bound page/pageSize.

### III. Event-Driven Integration

Domain events are `...DomainEvent : IDomainEvent` in
`Gym.Domain/<Module>/<Module>Events.cs` and are raised only by the aggregate via
`AddDomainEvent`. Do not publish integration events from command handlers. The
chain is: `DomainEvent` -> bridge `INotificationHandler<...DomainEvent>`
(`Application/<Module>/Events`) -> `...IntegrationEvent`
(`Application/Common/Events/<Module>`) -> `IConsumer<...IntegrationEvent>`
(`Infrastructure/<Module>/Consumers`). The bridge may read repositories to
enrich the contract but MUST NOT perform side effects; side effects (email,
external calls) belong to consumers only. Consumer class names MUST be unique
within the assembly (queue name equals type name without namespace). Use
`IIntegrationEventPublisher`/`IPublishEndpoint`, never `IBus`. State changes go
through the Bus Outbox; consumers use the Consumer Outbox/Inbox. Never publish
directly to the broker. `...IntegrationEvent` is a stable contract.

### IV. EF Core, PostgreSQL and Data Integrity

Inspect entity configurations and migrations before editing. Avoid N+1 queries
and unnecessary tracking. Consider transactions and concurrent updates. Never
drop, reset, or delete data without approval. Never silently rewrite migrations
that may already be applied; add a new migration. `DispatchDomainEventsInterceptor`
MUST run on `SavingChangesAsync` (before commit) so outbox writes share the
transaction; do not move it to `SavedChangesAsync` while the Bus Outbox is
enabled. `ExecuteUpdate`/`ExecuteDelete` bypass domain events; for aggregate
changes that must raise events, load the entity, call the domain method, and
`SaveChanges`. Use bulk operations only for technical/idempotent work. Guid IDs
are domain-assigned, so every entity config uses `ValueGeneratedNever()`. All
timestamps are UTC and stored as `timestamptz`. Subscription periods are
separate, non-overlapping rows (prepayment allowed); renewing creates a new
period. `Payment` is a separate aggregate, and a purchase records the
subscription and payment in one transaction.

### V. Verification, Security and Git Safety

Never claim a build or test passed unless the command was actually run; report
the commands and their results. There is currently no test project - do not
invent test commands; when tests are added, run the relevant project. Validate
external input at the application boundary and do not swallow exceptions. Keep
secrets (DB, SMTP, RabbitMQ) in User Secrets locally and `.env` (git-ignored)
for Docker Compose; never commit `.env` or secrets and never log passwords,
tokens, connection strings, or credentials. Inspect `git status` and `git diff`
before changes, do not overwrite unrelated user changes, do not run destructive
Git commands without approval, and do not commit or push unless explicitly
requested.

## Technology Stack and Constraints

- .NET 10 (`net10.0`) with nullable reference types and implicit usings enabled;
  ASP.NET Core Web API.
- MediatR 14.1.0 for CQRS and in-process domain events.
- EF Core 10.0.9 with Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2 for writes,
  migrations, and the transactional outbox.
- Dapper 2.1.79 and Npgsql 10.0.3 for read queries in the Application layer.
- MassTransit 8.x (currently resolved to 8.5.10) with RabbitMQ and the EF Core
  outbox/inbox. MassTransit 9.x is forbidden while it requires a commercial
  license.
- Hangfire 1.8.x (`Hangfire.AspNetCore` 1.8.25, `Hangfire.PostgreSql` 1.21.1)
  for in-app background jobs.
- MailKit 4.18.2 for SMTP sending with one client per send. FluentEmail 3.0.2 is
  still referenced but is legacy.
- AutoMapper 16.1.1 is referenced but currently unused; do not rely on it.
- PostgreSQL as the database, RabbitMQ as the broker, Docker Compose for local
  infrastructure, and User Secrets for local configuration.
- On the development machine a native PostgreSQL may own `localhost:5432` while
  the compose `postgres` service stays unused; confirm the target database
  before diagnosing data issues.

## Development Workflow and Quality Gates

- Inspect the relevant code before changing it; for non-trivial work, analyze and
  propose a plan first. Implement only the requested scope and do not refactor
  unrelated code.
- Build with `dotnet build GymAndTrainingApi.slnx`.
- Manage schema with `dotnet ef migrations add <Name> --project Gym.Infrastructure
  --startup-project GymAndTrainingApi` and `dotnet ef database update --project
  Gym.Infrastructure --startup-project GymAndTrainingApi`; set
  `ASPNETCORE_ENVIRONMENT=Development` so User Secrets load.
- Spec-driven features follow the Spec Kit flow: specify -> clarify -> plan ->
  tasks -> implement. Use clarify after specify to resolve ambiguities before
  planning, and review the spec and plan before tasks and implementation.
- Commit only when explicitly requested, with a message that matches the
  repository style.

## Governance

This constitution defines non-negotiable project principles. `AGENTS.md`
provides detailed runtime guidance and MUST remain consistent with this
document; if they conflict, the constitution wins and `AGENTS.md` is updated.
Amendments require a documented change (pull request or spec), and versioning
follows semantic versioning: MAJOR for incompatible governance or principle
removals/redefinitions, MINOR for new or materially expanded
principles/sections, PATCH for clarifications. Every review MUST verify
compliance, and any added complexity MUST be justified.

**Version**: 1.1.0 | **Ratified**: 2026-11-02 | **Last Amended**: 2026-11-02
