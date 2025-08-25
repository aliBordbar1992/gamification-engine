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

* [ ] **Set up Background Service Testing Infrastructure**
  * Create test harness for `EventQueueBackgroundService`
  * Implement test event queue with configurable processing
  * Set up test timing and synchronization utilities
  * Create test scenarios for background processing

* [ ] **Implement Test Assertion Utilities**
  * Create custom assertion helpers for domain entities
  * Implement JSON response validation utilities
  * Create database state assertion helpers
  * Set up test result reporting and debugging tools

* [ ] **Create Performance Testing Infrastructure**
  * Set up test metrics collection and reporting
  * Implement load testing utilities for API endpoints
  * Create performance baseline testing
  * Set up test execution time monitoring

* [ ] **Wire Up Current API Endpoints for E2E Testing**
  * Implement comprehensive E2E tests for `POST /api/events`
  * Test event ingestion with database persistence validation
  * Test event retrieval endpoints (`GET /api/events/user/{userId}`, `GET /api/events/type/{eventType}`)
  * Validate JSON response formats and HTTP status codes
  * Test error handling and validation scenarios
  * Verify background service event processing

**Deliverable:** Complete E2E testing infrastructure that validates the entire stack from HTTP requests through database persistence, with reusable components for future development phases.

---

### **Phase 3 – Condition Engine**

* [ ] **Define Condition base interface**.
* [ ] **Implement core condition types**:

  * `alwaysTrue`
  * `attributeEquals`
  * `count`
  * `threshold`
  * `sequence`
  * `timeSinceLastEvent`
  * `firstOccurrence`
  * `customScript` (optional sandboxed code)
* [ ] **Condition evaluation framework** (all/any logic).
* [ ] **Unit tests for each condition type**.

**Deliverable:** Any condition type can be evaluated against stored events.

---

### **Phase 4 – Rules Engine**

* [ ] **Define Rule entity** (triggers + conditions + rewards).
* [ ] **Implement Rule evaluation lifecycle** (trigger → fetch conditions → check → execute rewards).
* [ ] **Add configuration for rules** (YAML/JSON).
* [ ] **Support multiple triggers per rule**.
* [ ] **Logging for rule execution**.

**Deliverable:** Engine can process a rule end-to-end and issue rewards.

---

### **Phase 5 – Rewards System**

* [ ] **Points awarding logic** (multiple categories).
* [ ] **Badge awarding logic** (once per badge).
* [ ] **Trophy awarding logic** (meta-badges).
* [ ] **Level progression** (based on point thresholds).
* [ ] **Penalty application** (points, badges, level).
* [ ] **Store and retrieve user reward history**.

**Deliverable:** Rules can give points, badges, trophies, levels, and penalties.

---

### **Phase 6 – Entity Definitions**

* [ ] **Define schema for point categories**.
* [ ] **Define schema for badges** (ID, name, description, icon).
* [ ] **Define schema for trophies**.
* [ ] **Define schema for levels**.
* [ ] **Load entity definitions from config**.
* [ ] **Expose CRUD API for admin to manage these entities**.

**Deliverable:** Admin can define rewards and use them in rules.

---

### **Phase 7 – Leaderboards**

* [ ] **Implement leaderboard generation logic** (per category, time range).
* [ ] **In-memory first, then cached DB implementation**.
* [ ] **API to query leaderboards** (`GET /leaderboards`).
* [ ] **Support pagination & filters**.
* [ ] **Unit tests for leaderboard rankings**.

**Deliverable:** Leaderboards show correct rankings for different time ranges.

---

### **Phase 8 – API & Integration Layer**

* [ ] **REST API for all major functions**:

  * Ingest events
  * Get user points, badges, trophies, levels
  * Get leaderboards
  * Manage rules & entities
* [ ] **Webhook system** (notify external systems when rewards are issued).
* [ ] **SDK for JS/TS** (optional).
* [ ] **Swagger/OpenAPI documentation**.

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

* [ ] **Plugin interface for new condition types**.
* [ ] **Plugin interface for new reward types**.
* [ ] **Performance optimization** (caching, async execution).
* [ ] **Scalable event queue adapter** (Kafka, SQS, etc.).
* [ ] **Stress tests** for high event volume.

**Deliverable:** Engine is production-ready, scalable, and easy to extend.
