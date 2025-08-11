# Contributing to Gamification Engine

Thank you for your interest in contributing!

## Architecture
- Follow Clean Architecture / Hexagonal. Domain cannot depend on Application or Infrastructure.
- All external dependencies must be injected via interfaces (ports). No IO or logging in Domain.
- Rules logic is declarative YAML/JSON loaded via factories.

## Coding Standards
- .NET 9, C# 13, multi-project solution.
- SOLID, DDD. Entities/aggregates have no public setters; enforce invariants via methods.
- No mutable static or global state. No side-effects in constructors.
- Public interfaces and methods must include concise XML comments.
- Use Result<T, E> for recoverable failures. Use domain-specific exceptions for invariant violations.

## Testing
- TDD is mandatory for domain and application layers.
- Use xUnit + Shouldly for unit tests.
- Add BDD-style tests for complex rule scenarios when applicable.
- Run `dotnet test` locally; ensure green before PR.

## Commit & PR Guidelines
- Small, focused commits. Reference the related `todos.md` item.
- PRs must pass CI, include tests, and adhere to architecture boundaries.
- No business logic in controllers or infrastructure layers.

## Folder Structure
- `src/Domain` — Entities, Value Objects, Repositories, Domain Services
- `src/Application` — Ports, DTOs, Orchestrators, Validators
- `src/Infrastructure` — Adapters (Configuration, Storage, etc.)
- `tests` — Unit and integration tests

## Local Development
1. Restore: `dotnet restore`
2. Build: `dotnet build`
3. Test: `dotnet test`

By contributing, you agree to follow these guidelines. Thank you! 