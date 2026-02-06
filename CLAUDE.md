# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet build                                    # Build entire solution
dotnet test                                     # Run all tests
dotnet test --filter "FullyQualifiedName~CategoriesSpec"  # Run a single test class
dotnet test --filter "DisplayName~Should create a new category"  # Run a single test by name
dotnet run --project Cetus.Api                  # Run the API locally
```

The CI pipeline runs: `dotnet restore` → `dotnet build --no-restore` → `dotnet test --no-build`

**Build strictness:** `TreatWarningsAsErrors` is enabled globally (Directory.Build.props), along with SonarAnalyzer.CSharp and full code analysis. All warnings are errors.

## Architecture

Clean Architecture with CQRS, targeting **.NET 10** with **PostgreSQL**.

### Layer dependency flow: `Cetus.Api → Application → Domain ← SharedKernel`; `Infrastructure → Application`

**SharedKernel/** — Base abstractions: `Entity` (with domain event support), `Result<T>`/`Error` (railway-oriented error handling), `IDomainEvent`, `IDomainEventHandler<T>`, `PagedResult<T>`.

**Domain/** — DDD aggregates and entities. Key bounded contexts: Orders (state machine with `AllowedTransitions`), Products (with PostgreSQL tsvector full-text search), Categories, Coupons, Reviews, Stores. Uses soft deletes (`DeletedAt`), value objects (`Customer`, `DeliveryFee`), and domain error factory methods (`OrderErrors.cs`).

**Application/** — CQRS without MediatR. Custom `ICommand<T>`/`ICommandHandler<T,TResponse>` and `IQuery<T>`/`IQueryHandler<T,TResponse>` interfaces. Handlers are auto-registered via Scrutor assembly scanning. Cross-cutting concerns via decorators: `ValidationDecorator` (FluentValidation) → `LoggingDecorator` → Handler. No repository pattern — handlers use `IApplicationDbContext` (EF Core) directly.

**Infrastructure/** — PostgreSQL via EF Core with snake_case naming convention, PostgreSQL enum type mappings, domain event dispatching (Channel-based pub/sub with `DomainEventsPooler` HostedService), Resend email, MercadoPago/Wompi payments, AWS S3, FusionCache, Quartz scheduling, OpenTelemetry, JWT auth with JWKS.

**Cetus.Api/** — ASP.NET Core Minimal APIs. Endpoints implement `IEndpoint.MapEndpoint()` and are auto-discovered. All routes under `/api` require authorization, CORS, and rate limiting. Multi-tenancy via `TenantResolverMiddleware` (resolves store from Referer/Origin headers). SignalR hub at `/api/realtime/orders`. Error responses use RFC 7231 ProblemDetails via `CustomResults.Problem()`.

## Key Patterns

### Adding a new feature (CQRS command)

Create three files in `Application/{Feature}/{Action}/`:
1. `{Action}{Feature}Command.cs` — `sealed record` implementing `ICommand<TResponse>`
2. `{Action}{Feature}CommandHandler.cs` — `internal sealed class` implementing `ICommandHandler<TCommand, TResponse>`
3. `{Action}{Feature}CommandValidator.cs` — `AbstractValidator<TCommand>` (FluentValidation)

Then create the endpoint in `Cetus.Api/Endpoints/{Feature}/{Action}.cs` implementing `IEndpoint`. The handler is injected directly into the endpoint lambda. Use `result.Match(Results.Ok, CustomResults.Problem)` for response mapping.

Queries follow the same pattern with `IQuery<T>` / `IQueryHandler<T, TResponse>`.

### Error handling

Business errors use `Result<T>` — never throw exceptions for domain failures. Define domain errors as static methods in `{Entity}Errors.cs` using `Error.NotFound()`, `Error.Problem()`, `Error.Conflict()`, etc. `CustomResults.Problem()` maps `ErrorType` to HTTP status codes (Validation/Problem→400, NotFound→404, Conflict→409).

### Multi-tenancy

Every query/command has access to `ITenantContext.Id` (the store's Guid). The tenant is resolved from request headers by middleware and cached for 5 hours.

### Domain events

Entities inherit from `Entity` and call `Raise(new SomeDomainEvent())`. Events are dispatched **after** `SaveChangesAsync` (eventual consistency). Handlers implement `IDomainEventHandler<T>` and are auto-registered.

## Testing

Integration tests using `WebApplicationFactory<Program>`. Test classes extend `ApplicationContextTestCase` (which wraps `ApplicationTestCase` as `IClassFixture`). Tests use:
- **xUnit** with `[Fact(DisplayName = "...")]`
- **Shouldly** for assertions
- **Bogus** for fake data (via Faker classes in `Shared/Fakers/`)
- **Moq** for service mocking (IResend, IDateTimeProvider are mocked in test setup)
- Test auth scheme bypasses JWT
- Tests hit HTTP endpoints via `Client.PostAsJsonAsync("api/...")` (not handler unit tests)

Test files are named `{Feature}Spec.cs` at the root of the test project.

## Code Style

- File-scoped namespaces (enforced as error)
- `var` when type is apparent; explicit types for built-in types (`int`, `string`, etc.)
- Braces required (enforced as error)
- Handlers and endpoints are `internal sealed`
- Commands/queries are `sealed record` types
- Response DTOs use static `FromEntity()` factory methods (no AutoMapper)
- Validation messages are in Spanish
- 4-space indentation, CRLF line endings
