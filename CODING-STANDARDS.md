# Coding Standards

- Use .NET 9, C# 13 features judiciously; prefer clarity over cleverness.
- Apply SOLID and DDD principles. Encapsulate invariants inside aggregates; avoid public setters.
- Domain layer: no IO, logging, or external dependencies. No side effects in constructors.
- Use Value Objects when appropriate; avoid primitive obsession.
- Errors:
  - Prefer Result<T, E> for expected failures.
  - Throw domain-specific exceptions only for invariant violations.
  - Handle/wrap infrastructure exceptions at boundaries.
- Asynchrony: always await async calls; propagate or handle explicitly.
- Logging: only in infrastructure; use structured logging with metadata.
- Rules: define in YAML/JSON; no embedded scripts.
- Naming: clear, explicit names for entities, events, services, and repositories.
- Tests: TDD; use xUnit + Shouldly; ensure deterministic, isolated tests. 