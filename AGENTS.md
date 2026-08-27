# Repository Guidelines

## Project Structure & Module Organization

This .NET 8 solution contains two independently layered services: `src/ProductService/` and `src/OrderService/`. Each has `Domain`, `Application`, `Infrastructure`, and `Api` projects. Keep domain entities and repository contracts dependency-free; put CQRS requests, DTOs, handlers, and validators in `Application`; EF Core/PostgreSQL implementations in `Infrastructure`; and HTTP endpoints, middleware, and dependency registration in `Api`.

Tests mirror services in `tests/ProductService.Tests/` and `tests/OrderService.Tests/`. Shared build settings are in `Directory.Build.props`; local PostgreSQL is defined in `docker-compose.yml`. See `CONVENTIONS.md` for the full architectural rules.

## Build, Test, and Development Commands

- `dotnet restore EcommerceMicroservices.sln` restores packages.
- `dotnet build EcommerceMicroservices.sln` compiles all projects.
- `dotnet test EcommerceMicroservices.sln` runs the xUnit suite.
- `dotnet run --project src/ProductService/ProductService.Api` starts the Product API; replace `Product` with `Order` for the other API.
- `docker compose up -d` starts PostgreSQL on `localhost:5432` (database/user/password: `ecommercedb`/`postgres`/`postgres`).

## Coding Style & Naming Conventions

Use C# 12 with nullable references and implicit usings enabled. Follow standard .NET formatting (four-space indentation) and keep one public type per file, with file and type names matching exactly. Use PascalCase for types and members; prefix interfaces with `I`. Define DTOs, commands, and queries as immutable `public record` types. Name MediatR handlers `<Request>Handler`, validators `<Command>Validator`, and asynchronous I/O methods with an `Async` suffix.

Use lowercase plural API routes such as `/api/products`. Return errors through the global `ProblemDetails` middleware rather than ad-hoc response shapes.

## Testing Guidelines

Write xUnit tests using Moq, FluentAssertions, and EF Core InMemory where appropriate. Follow Arrange/Act/Assert with explicit comments, and name tests `MethodName_Scenario_ExpectedResult`, e.g. `CreateProduct_WithValidData_ReturnsCreatedResult`. Add or update tests with every behavior change; run the full solution test command before opening a PR.

## Commit & Pull Request Guidelines

Use concise Conventional Commit-style subjects, following the existing `feat(setup): initialize solution structure and docker configuration` pattern. Prefer scopes such as `product`, `order`, or `infra`. PRs should state the affected service and behavior, link the relevant issue when available, include test results, and attach request/response evidence or screenshots for API-facing changes.

## Configuration & Security

Keep secrets out of `appsettings*.json` and source control. Use environment variables or local secret storage for connection strings and credentials; never commit production values.

## Git Workflow

- El repositorio de Github de este proyecto es: `ngomezleal/EcommerceMicroservices`
- Nunca trabajes directamente sobre `master`
- Antes de implementar cualquier feature, bugfix o issue, verifica la rama actual.
- Si la rama actual es `master`, crea una nueva antes de realizar cambios.
- Usa nombres descriptivos:
    - feature/<description>
    - fix/<description>
    - refactor/<description>
- Realiza todos los commit en la nueva rama.
- Nunca hagas push directo a `master`.
- Nunca hagas commit al menos que se te indique hacerlo de manera explicita.