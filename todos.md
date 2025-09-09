## **Project Description**

We are building a **Headless, Modular, Cross-Platform Gamification Engine** that can be embedded into various products (e.g., book-reading apps, e-commerce platforms, educational tools) to provide a consistent way of awarding **Points**, **Badges**, **Trophies**, **Levels**, **Leaderboards**, and **Penalties** based on user actions.

### **Core Capabilities**

1. **Events ingestion**

   * The engine consumes platform-specific user activity events (e.g., “USER_COMMENTED”, “PRODUCT_PURCHASED”) from any source via API/webhooks/SDK.
   * Events can carry custom attributes (e.g., "purchaseSource": "special_offers") for rules to evaluate.

2. **Rules engine**

   * Admins configure **rules** linking triggers (events) + conditions → rewards.
   * Rules use a standard vocabulary of condition types (sequence, count, attributeEquals, threshold, etc.).
   * Rules are **data-driven** (YAML/JSON), so the logic can be changed without code changes.

3. **Rewards system**

   * Supports different reward types:

     * Points (multiple categories: XP, Score, Coins)
     * Badges
     * Trophies
     * Level ups
     * Penalties (point reduction, badge removal, restrictions)

4. **Entity definitions**

   * Badges, trophies, point categories, and levels are defined independently and referenced by rules.
   * Allows easy reuse across different rules and events.

5. **Composability of actions**

   * Multiple events can be combined with temporal/spatial conditions (e.g., “visit special offers page” AND “purchase within 30 minutes” → reward).

6. **Leaderboards**

   * Configurable by point category, time window (daily, weekly, all-time), and filters.
   * Supports real-time or scheduled updates.

7. **Extensibility & Modularity**

   * Each concept (points, badges, leaderboards, penalties) can be enabled or disabled.
   * New condition types or reward types can be plugged in without affecting others.

8. **Headless API**

   * REST/GraphQL endpoints for integrating with any frontend.
   * Webhooks for external notifications when rewards are issued.
   * Optional SDKs for common platforms.

---

## **Development Cycle TODO List**

Organized into **phases** with clear, testable boundaries so that AI coding tools (like Cursor) can pick them up easily.

---

### **Phase 1 – Project Foundations**

* [x] **Set up repository structure** (monorepo or modular folders: `core`, `api`, `rules`, `storage`, `examples`).
* [x] **Decide on language, framework, and package management**.
* [x] **Implement configuration loader** (YAML/JSON parser with validation).
* [x] **Define domain model base classes**:

  * Event
  * Condition
  * Reward
  * Rule
  * UserState (points, badges, etc.)
* [x] **Add pluggable storage interface** (abstract for DB; in-memory adapter for testing).
* [x] **Create base test harness** with automated test runner.
* [x] **Document coding conventions** and contribution guidelines.

**Deliverable:** Barebones project that loads config files, passes schema validation, and runs a dummy test.

---

### **Phase 2 – Event System**

* [x] **Implement Event class** with metadata and attributes.
* [x] **Add event ingestion API** (REST endpoint: `POST /events`).
* [x] **Support event queue** (in-memory first, later pluggable for Kafka/RabbitMQ/etc.).
* [x] **Store events in DB** with retention policy.
* [x] **Test ingestion with multiple event types**.
* [x] **Write Unit Tests using Shouldly for Event related classes in Domain layer**
* [x] **Write integration tests for API and Application services**
* [x] **Create ".http" test files for API**

**Deliverable:** Events can be sent to the engine and retrieved via API.

---

### **Phase 2.1 – E2E Testing Infrastructure**

* [x] **Set up Integration Test Project Structure**
  * Create `tests/Integration.Tests/` project with xUnit + TestServer
  * Configure WebApplicationFactory for full application testing
  * Set up test project references and dependencies

* [x] **Implement Test Database Infrastructure**
  * Create `TestDbContext` with configurable database providers
  * Support multiple test database types (InMemory, PostgreSql)
  * Implement database seeding and cleanup utilities
  * Create test data factories for events, users, and configurations

* [x] **Set up Test Configuration Management**
  * Create test-specific `appsettings.Testing.json`
  * Implement configuration overrides for test environment
  * Set up test logging and monitoring
  * Configure test-specific connection strings and settings

* [x] **Create Test Infrastructure Abstractions**
  * Implement `ITestDatabase` interface for database management
  * Create `TestDataBuilder` for generating test data
  * Implement `TestHttpClientFactory` for HTTP testing
  * Set up test lifecycle management (setup/teardown)

* [x] **Implement Test Data Management**
  * Create test data fixtures for common scenarios
  * Implement database state reset between tests
  * Set up test data isolation and parallel execution support
  * Create test data validation utilities

* [x] **Set up Background Service Testing Infrastructure**
  * Create test harness base for background processing
  * Set up test timing and synchronization utilities
  * Create test scenarios for background processing

* [x] **Implement Test Assertion Utilities**
  * Create custom assertion helpers for domain entities
  * Implement JSON response validation utilities
  * Create database state assertion helpers
  * Set up test result reporting and debugging tools

* [x] **Create Performance Testing Infrastructure**
  * Set up test metrics collection and reporting
  * Implement load testing utilities for API endpoints
  * Create performance baseline testing
  * Set up test execution time monitoring

* [ ] **Wire Up Current API Endpoints for E2E Testing**
  * Implement comprehensive E2E tests for all endpoints in EventsController
  * Test event ingestion with database persistence validation (postgres)
  * Validate JSON response formats and HTTP status codes
  * Test error handling and validation scenarios
  * Ensure that tests are using **Existing** infrastructures and utilities existing in `/tests/Integration.Tests/Infrastructure` folder
  * This test should work as a comprehensive example of how to use all aspects of infrastructures implemented in tasks 1 through 8 in this phase

**Deliverable:** Complete E2E testing infrastructure that validates the entire stack from HTTP requests through database persistence, with reusable components for future development phases.

---

### **Phase 3 – Condition Engine**

* [x] **Define Condition base interface**.
* [x] **Implement core condition types**:

  * `alwaysTrue`
  * `attributeEquals`
  * `count`
  * `threshold`
  * `sequence`
  * `timeSinceLastEvent`
  * `firstOccurrence`
  * `customScript` (optional sandboxed code)
* [x] **Condition evaluation framework** (all/any logic).
* [x] **Unit tests for each condition type**.

**Deliverable:** Any condition type can be evaluated against stored events.

---

### **Phase 4 – Rules Engine**

* [x] **Define Rule entity** (triggers + conditions + rewards).
* [x] **Implement Rule evaluation lifecycle** (trigger → fetch conditions → check → execute rewards).
* [x] **Add configuration for rules** (YAML/JSON).
* [x] **Support multiple triggers per rule**.
* [x] **Logging for rule execution**.

**Deliverable:** Engine can process a rule end-to-end and issue rewards.

---

### **Phase 5 – Rewards System**

* [x] **Points awarding logic** (multiple categories).
* [x] **Badge awarding logic** (once per badge).
* [x] **Trophy awarding logic** (meta-badges).
* [x] **Level progression** (based on point thresholds).
* [x] **Penalty application** (points, badges, level).
* [x] **Store and retrieve user reward history**.

**Deliverable:** Rules can give points, badges, trophies, levels, and penalties.

---

### **Phase 6 – Entity Definitions**

* [x] **Define schema for point categories**.
* [x] **Define schema for badges** (ID, name, description, icon).
* [x] **Define schema for trophies**.
* [x] **Define schema for levels**.
* [x] **Load entity definitions from config**.
* [x] **Expose CRUD API for admin to manage these entities**.

**Deliverable:** Admin can define rewards and use them in rules.

---

### **Phase 7 – Leaderboards**

* [x] **Implement leaderboard generation logic** (per category, time range).
* [x] **In-memory first, then cached DB implementation**.
* [x] **API to query leaderboards** (`GET /leaderboards`).
* [x] **Support pagination & filters**.
* [x] **Unit tests for leaderboard rankings**.
* [x] **Implement proper time range filtering for leaderboards**:
  * Create `IRewardHistoryRepository` interface and implementation for tracking reward timestamps
  * Keep `UserState` as aggregate root representing current state only (no history tracking)
  * Create separate `RewardHistory` aggregate/entity to track when rewards were earned
  * Update `FilterUsersByTimeRangeAsync()` to query `RewardHistory` for time-based filtering
  * Implement time-based filtering logic that respects `daily`, `weekly`, `monthly`, and `alltime` ranges
  * Add integration tests to verify time range filtering works correctly with real timestamp data
  * Rewards should update `UserState` as they are earned by user
  * Ensure leaderboard queries properly filter users based on when they earned their points/badges/trophies

**Deliverable:** Leaderboards show correct rankings for different time ranges with proper time-based filtering.

---

### **Phase 8 – API & Integration Layer**

* [x] **REST API for all major functions**:

  * Ingest events
  * Get user points, badges, trophies, levels
  * Get leaderboards
  * Manage rules & entities
* [x] **Webhook system** (notify external systems when rewards are issued).
* [ ] **SDK for JS/TS** (optional).
* [x] **Swagger/OpenAPI documentation**.

**Deliverable:** External systems can fully integrate with the engine.

---

### **Phase 9 – Admin Panel (Optional)**

* [ ] **Basic web UI** for rules, badges, leaderboards.
* [ ] **User search** with activity/rewards view.
* [ ] **Test rule execution in sandbox mode**.
* [ ] **Export/import configurations**.

**Deliverable:** Usable admin tool for non-technical operators.

---

### **Phase 10 – Extensibility & Optimization**

* [x] **Plugin interface for new condition types**.
* [x] **Plugin interface for new reward types**.
* [x] **Performance optimization** (caching, async execution).
* [x] **Scalable event queue adapter** (Kafka, SQS, etc.).
* [x] **Stress tests** for high event volume.

**Deliverable:** Engine is production-ready, scalable, and easy to extend.

---

### **Phase 11 – Configuration Enforcement & Operationalization**

* [ ] Implement rule cooldowns and one-time enforcement
  * Add `ICooldownService` and storage (per-user, per-rule) to track last award timestamp and counts
  * Enforce `cooldowns` from config (`minMinutesBetweenAwards`, `one_time`) before executing rewards
  * Respect `RuleMetadata.OneTime` and `RuleMetadata.Repeatable` during evaluation
  * Add unit tests covering daily, once-only, and edge-cases

* [ ] Wire notifications and webhooks to reward lifecycle
  * Load `notifications.templates` from config and implement simple token templating (e.g., `{{amount}}`, `{{badgeName}}`)
  * Publish a `RewardAwarded` domain event after successful reward execution
  * Handle `RuleMetadata.NotifyPlatform` and `RuleMetadata.WebhookOnAward` to send notifications via `IWebhookService`
  * Add retry/backoff for webhook failures and capture delivery results
  * Add unit tests for template rendering and webhook dispatch paths

* [ ] Add API key authentication and role-based authorization
  * Implement API key middleware reading `engine.security.admin_api_keys` and `engine.security.sdk_api_keys`
  * Map keys to roles (Admin/Operator/SDK) and apply policy-based `[Authorize]` on controllers/actions
  * Protect management endpoints (rules/entities) for Admin, allow event ingestion for SDK
  * Add authentication/authorization unit tests for all protected endpoints

* [ ] Support processing modes: sync, async, hybrid
  * Read `engine.processing.mode` from config
  * Sync: evaluate rules inline in `EventIngestionService`
  * Async: process in `EventQueueBackgroundService` (store event + evaluate rules)
  * Hybrid: execute lightweight rewards synchronously, queue heavy work (e.g., leaderboard rebuilds)
  * Add unit tests verifying each mode behavior and idempotency

* [ ] Implement event replay capability
  * Create `IEventReplayService` to re-evaluate rules for a user and/or time range
  * Add API endpoint `POST /api/events/replay` with filters (userId, from, to, eventTypes)
  * Ensure replay respects cooldowns/one-time semantics with an override flag option
  * Add unit tests for replay correctness and safety

* [ ] Add audit logging and retention based on config
  * Implement audit entries (reuse `RewardHistory` or add dedicated `RewardAudit`) controlled by `engine.audit.enable`
  * Add retention job honoring `engine.audit.audit_retention_days`
  * Record rule evaluations and reward outcomes with correlation IDs
  * Add unit tests validating retention and audit toggling

* [ ] Extend configuration model and loader
  * Project missing config sections: `engine.security`, `engine.audit`, `engine.processing`, `cooldowns`, `notifications`, `extensions`
  * Validate configuration at startup with actionable errors
  * Update `DatabaseSeederService` to apply full configuration consistently across repositories
  * Map YAML to C# models explicitly:
    * `engine.processing`: `mode`, `async_queue`, `workers` (points/badge/trophy/leaderboard), `replay_enabled`
    * `engine.security`: `admin_api_keys`, `sdk_api_keys`, `role_based_access` (roles → permissions)
    * `engine.audit`: `enable`, `audit_table`, `audit_retention_days`
    * `events[].payload_schema`: capture as `Dictionary<string,string>`
    * `penalties[]`: definition model for penalty triggers and actions
    * `leaderboards[]`: `id`, `name`, `category`, `scope`, `update_strategy`, `top_n`, `time_window_days`, `cache{type,key_prefix}`
    * `cooldowns[]`: `id`, `rule_id`, `minMinutesBetweenAwards`, `one_time`
    * `notifications`: `templates{}`, `webhook.on_reward_award{enabled,url,method,auth,token,payload_template}`
    * `extensions`: `custom_functions[]` and `webhook_conditions[]`
    * `simulation`: `enabled`, `sandbox_user_id`, `default_event_batch[]`
    * `monitoring`: `metrics[]` and `alerts[]`
  * Add tests for configuration binding and validation failures

* [ ] Enforce and expose event catalog
  * Persist configured `events` into a repository (e.g., `IEventDefinitionRepository`)
  * Validate incoming events against catalog: reject unknown with 400, or accept+warn (configurable)
  * Add `GET /api/events/catalog` endpoint to return configured events (including `payload_schema` when available)
  * Add unit tests for ingestion validation and catalog retrieval

* [ ] Expose metrics and health endpoints
  * Emit counters/histograms named in `monitoring.metrics` (e.g., `rewards_awarded_total`, `rules_evaluated_total`)
  * Add `/health` and `/metrics` endpoints and basic alerts scaffolding hooks
  * Add tests verifying metrics emission on key code paths

* [ ] Add simulation endpoint (optional enable)
  * If `simulation.enabled`, add `POST /api/simulation/run` to process `simulation.default_event_batch`
  * Ensure execution honors processing mode and cooldown settings
  * Add tests for simulation flows

**Deliverable:** Configuration-defined behaviors (cooldowns, notifications/webhooks, RBAC, processing modes, replay, audit, and metrics) are enforced end-to-end, validated by tests, and controllable solely via configuration without code changes.

---

### **Phase 12 – Testing Platform Consolidation and Quality Gates**

* [ ] Establish test suite baseline and triage
  * Run all test projects with detailed logging and collect failures/flaky tests
  * Produce a test inventory (unit/integration/e2e/perf/stress) with owners and gaps
  * Create a stabilization backlog for flaky tests and mark with `[Trait("Flaky", "true")]`

* [ ] Unify testing conventions and structure
  * Standardize on xUnit + Shouldly for assertions and Moq for mocks
  * Move integration-style tests from `tests/Application.Tests` into `tests/Integration.Tests`
  * Introduce `[Trait]` categories: `Unit`, `Integration`, `E2E`, `Performance`, `Stress`
  * Align `TargetFramework` and package versions across test projects (optionally via `Directory.Packages.props`)

* [ ] Unit testing improvements
  * Add time abstraction (e.g., `ISystemClock`) to remove `DateTime.UtcNow` dependencies
  * Replace sleeps with deterministic `ITestTimer`; remove nondeterminism and race conditions
  * Increase coverage for complex services (rewards, rule evaluation, leaderboards)
  * Use `AutoFixture`/`Bogus` for stable data generation with fixed seeds

* [ ] Integration testing hardening
  * Adopt `WebApplicationFactory<Program>` as the single entry for API integration tests
  * Use Testcontainers for PostgreSQL; parameterize provider (InMemory vs PostgreSQL) via `appsettings.Testing.json`
  * Ensure schema/migrations run for PostgreSQL containers prior to tests
  * Reset DB state between tests (`TestDataIsolationManager` or `Respawn` equivalent)
  * Provide helpers to seed YAML configuration on startup for integration runs

* [ ] End-to-end (API → DB) scenarios
  * Add `tests/Integration.Tests/Tests/E2E` validating ingestion → evaluation → reward history → leaderboards (time windows)
  * Use PostgreSQL via Testcontainers; assert `RewardHistory`, `UserState`, ranking outputs
  * Cover error handling, cooldowns, and one-time rule behavior once implemented

* [ ] Performance testing
  * Add `BenchmarkDotNet` micro-benchmarks for hot paths (rule evaluation, repositories)
  * Define performance baselines/thresholds and export reports (Markdown/CSV) in CI
  * Keep `Performance.Tests`; convert targeted scenarios into benchmarks where suitable

* [ ] Stress and load testing
  * Add `NBomber` scenarios: sustained and spike loads for ingestion and evaluation
  * Define SLOs (p95 latency, error rate) and fail tests if thresholds are exceeded
  * Publish HTML/CSV reports as CI artifacts

* [ ] Tooling, reporting, and CI gates
  * Enforce coverage via Coverlet (Cobertura): Unit ≥ 80%, Integration ≥ 60%
  * CI: run `Unit` on PRs; `Integration` + `E2E` nightly; `Performance` + `Stress` scheduled
  * Publish test results, coverage, and perf/stress reports; annotate PRs on failures

* [ ] Evaluate and consolidate Integration test infrastructure
  * Keep: DB factories (InMemory/PostgreSQL), config manager, HTTP client factory, data builders, monitoring utilities
  * Improve: ensure tests actually use these helpers; remove dead/duplicate utilities; enhance `Infrastructure/README.md`
  * Deprecate: redundant base classes; merge `IntegrationTestBase` and `EndToEndTestBase` if overlapping

* [ ] External dependency mocking
  * Add `WireMock.Net` or HttpMessageHandler-based fakes for webhook/external HTTP
  * Provide helpers to assert outgoing webhook requests and payloads

* [ ] Flaky test stabilization policy
  * Quarantine `[Trait("Flaky", "true")]` in separate CI job; auto-open tracking issues with logs
  * Remove `Random` nondeterminism; isolate concurrency via `ITestTimingUtilities`

**Deliverable:** A stable, maintainable testing platform with unified conventions, green unit/integration/E2E suites, reproducible performance and stress testing with thresholds and reports, and CI quality gates enforcing coverage and reliability.