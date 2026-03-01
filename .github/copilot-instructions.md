# Copilot / AI Agent Instructions for CleanArchDDD

Purpose: provide concise, actionable context so an AI coding agent can be immediately productive in this repository.

- Project type: .NET 8 Clean Architecture + DDD + CQRS (MediatR) with EF Core and Serilog.
- Solution entry: `CleanArchDDD.sln` (root).

Big picture
- Layers: `Domain` (entities, aggregates, domain events), `Application` (MediatR handlers, validators, DTOs), `Infrastructure` (EF Core DbContext, repositories, UoW), `API` (controllers, middleware, DI wiring).
- Key bootstrap points:
  - `src/API/Program.cs` — app pipeline, Serilog, Swagger, API versioning, `AddApplication()` and `AddInfrastructure()` calls.
  - `src/Application/DependencyInjection.cs` — registers MediatR, FluentValidation, pipeline behaviors (logging, validation, performance).
  - `src/Infrastructure/DependencyInjection.cs` — configures `AppDbContext` (InMemory when no connection string), registers `IUnitOfWork`.

Important patterns & conventions
- CQRS with MediatR: handlers live under `src/Application/Features/*` (Commands, Queries). Use `AddMediatR(...)` in `Application/DependencyInjection`.
- Validation: FluentValidation registered via `AddValidatorsFromAssembly(...)` in `Application` — prefer validator classes next to handlers.
- Domain events: events are collected on `BaseEntity` and dispatched in `AppDbContext.SaveChangesAsync()` (see `src/Infrastructure/Persistence/AppDbContext.cs`). Dispatch happens after saving.
- Persistence:
  - `AppDbContext` defines DbSets and applies EF configurations via `ApplyConfigurationsFromAssembly(...)`.
  - `UnitOfWork` exposes repositories and transaction methods (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`). Repositories are in `Infrastructure/Persistence/Repositories`.
  - If `DefaultConnection` is missing, the app uses an In-Memory DB for local dev and tests (see `Infrastructure/DependencyInjection`).
- Logging: Serilog is configured in `src/API/Program.cs` and used via `UseSerilogRequestLogging()` middleware.
- Errors: GlobalExceptions handled via `API/Middleware/GlobalExceptionMiddleware.cs`.

Build / run / test
- Restore & build: `dotnet build CleanArchDDD.sln`
- Run API locally: `dotnet run --project src/API` (or use VS/IDE via the solution). Swagger UI is exposed at root in Development.
- Tests: `dotnet test` runs tests under `tests/`.
- Migrations: production DB uses SQL Server when `DefaultConnection` is set; migrations assembly is `AppDbContext`'s assembly. To generate migrations, run from `src/Infrastructure` context with proper connection string.

Where to make changes
- New features: implement handlers/DTOs under `src/Application/Features/...`, validators beside them, then update controllers in `src/API/Controllers` if exposing new endpoints.
- DI: add registrations in `Application/DependencyInjection` or `Infrastructure/DependencyInjection` and confirm `Program.cs` calls `AddApplication()` and `AddInfrastructure()`.

Quick references (examples)
- API bootstrap: `src/API/Program.cs`
- Application DI: `src/Application/DependencyInjection.cs`
- Infrastructure DI: `src/Infrastructure/DependencyInjection.cs`
- DbContext + domain events: `src/Infrastructure/Persistence/AppDbContext.cs`
- UnitOfWork: `src/Infrastructure/Persistence/UnitOfWork.cs`

Agent rules (practical guidance)
- Prefer minimal, focused changes. Follow existing layer boundaries: do not add persistence logic into `Application` or domain entities that require EF Core.
- Keep domain models persistence-agnostic; read/write mapping occurs via repositories and EF configurations in `Infrastructure`.
- When adding DB migrations, ensure `DefaultConnection` is set and migrations assembly is correct.
- When editing public API surface (controllers), update Swagger XML comments if present so OpenAPI stays accurate.

If anything here is unclear or you want more detail (examples, code snippets, or CI/build notes), tell me which area to expand.
