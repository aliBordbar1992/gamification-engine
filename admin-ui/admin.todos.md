## Admin Panel – Development Plan and TODOs

### Project Description
The Admin Panel is a minimal, fast, and safe web application for operating the Headless Gamification Engine. It focuses on read-first workflows, schema-validated edits of declarative JSON/YAML, and a sandbox to dry‑run events. No business rules live in the UI; it only consumes existing APIs, validates payloads against schemas, and submits changes for the engine to enact.

## Phases of Development

### Phase 1 – Foundations and Skeleton
Tasks:
* [x] Initialize project (Vite + TS, ESLint/Prettier, Vitest/RTL, Playwright).
* [x] Set up Ant Design theme, layout (Sider + Header + Content), responsive grid.
* [x] Configure React Router with lazy routes and error boundaries.
* [x] Integrate TanStack Query with retry/backoff and devtools.
* [x] Implement Axios client with base URL, auth, error interceptors.
* [x] Add `AuthProvider` with role model (Viewer, Operator, Admin) and route guards.
* [x] Create placeholder pages: Dashboard, Rules, Entities, Users, Leaderboards, Sandbox, Settings.

Deliverables:
- Running app with navigation, auth guard stubs, query provider, CI (lint/test/build).

Area of Impact:
- Developer experience, routing, theming, auth scaffolding.

---

### Phase 2 – Read‑Only Operations (Safe by Default)
Tasks:
* [x] Rules: list/search/filter; details page with read‑only JSON/YAML view.
* [x] Entities: lists (Badges, Trophies, Levels, Point Categories) + details (read‑only).
* [x] Users: ID lookup; show points by category, badges, trophies, level.
* [ ] Leaderboards: query by category + time range; table + optional mini chart.
* [ ] Implement API clients and query hooks for all above using existing engine endpoints.

Deliverables:
- Operators can browse rules, entities, users, and leaderboards without side effects.

Area of Impact:
- Stability, discoverability of existing engine state.

---

### Phase 3 – Safe Controls: Enable/Disable and Audit
Tasks:
* [ ] Rules: enable/disable toggle with optimistic UI and rollback on failure.
* [ ] Record audit metadata (actor, time, ruleId, action) via backend or client-side log + server endpoint.
* [ ] Confirmation modals and access control (Admin only for toggles).

Deliverables:
- Minimal yet essential operational control over rules with traceability.

Area of Impact:
- Operations, governance, audit trail.

---

### Phase 4 – Sandbox (Dry‑Run) with Evaluation Trace
Tasks:
* [ ] JSON/YAML event editor using Monaco with examples and schema hints.
* [ ] Submit event to a dry‑run endpoint (no persistence/side effects).
* [ ] Display evaluation trace: matched triggers, conditions results, rewards that would be issued.
* [ ] Copy-as-cURL export of the request.
* [ ] Persist last N sandbox inputs per user (localStorage only).

Deliverables:
- Safe experimentation environment to preview outcomes without affecting data.

Area of Impact:
- Developer/operator productivity; reduced production risk.

---

### Phase 5 – Rule Editor (Schema‑Validated JSON/YAML)
Tasks:
* [ ] Read/write editor with Monaco for JSON and YAML.
* [ ] JSON Schema validation via `ajv`; inline error markers; schema-driven intellisense.
* [ ] Draft → Validate → Publish workflow with diff viewer against current active version.
* [ ] Require change notes on publish; display version history (read-only list if backend supports).
* [ ] Guardrails: can’t publish with validation errors; Admin role only.

Deliverables:
- Safe editing of rules via declarative configs with strong validation and diff preview.

Area of Impact:
- Change management, correctness, velocity of rule updates.

---

### Phase 6 – Entities CRUD (Minimal)
Tasks:
* [ ] Create/edit basic fields for Badges, Trophies, Levels, Point Categories.
* [ ] Reuse form primitives with Zod validation; confirm on save.
* [ ] Optional: reference checks (e.g., show rules referencing an entity before delete).

Deliverables:
- Admins can minimally manage core entities directly from the UI.

Area of Impact:
- Content management, reduced friction for entity updates.

---

### Phase 7 – Import/Export and Versioning
Tasks:
* [ ] Export all rules/entities as JSON bundle.
* [ ] Import with schema validation and visual diff; require confirmation.
* [ ] Optional: bulk toggle operations with preview.

Deliverables:
- Portable configuration management; safer bulk updates.

Area of Impact:
- DevOps, migrations, environment parity.

---

### Phase 8 – Security, Roles, and Policies
Tasks:
* [ ] Implement role‑based access (Viewer/Operator/Admin) across routes and actions.
* [ ] Session handling (JWT renew, logout) and CSRF‑safe patterns if using cookies.
* [ ] Rate limit sandbox requests client-side (debounce) and display server messages.

Deliverables:
- Appropriate separation of duties; safer operations.

Area of Impact:
- Security posture, compliance readiness.

---

### Phase 9 – E2E Tests and Hardening
Tasks:
* [ ] Playwright E2E: navigation, rules toggle, sandbox dry‑run, rule edit publish, entity CRUD.
* [ ] Contract tests for API clients (using MSW for mocks where needed).
* [ ] Error states, loading skeletons, empty states; Lighthouse basic checks.

Deliverables:
- Confidence to ship; guard against regressions.

Area of Impact:
- Reliability, maintainability, developer confidence.

---

### Phase 10 – Deployment & Ops
Tasks:
* [ ] CI: lint, typecheck, unit tests, E2E smoke, build artifacts.
* [ ] CD: environment configs via `.env`; cache busting; error reporting hooks.
* [ ] Runtime config screen (readonly) to display backend base URL, build hash, feature flags.

Deliverables:
- Repeatable builds and deployments across environments.

Area of Impact:
- Operability, release cadence.

---

## Mapping to Engine Capabilities (API Touchpoints)
* [ ] Rules: list/get, enable/disable, update (publish), history (if available)
* [ ] Entities: list/get, create/update/delete (optional minimal)
* [ ] Users: get user state by ID
* [ ] Leaderboards: query by category + time range
* [ ] Sandbox: dry‑run evaluation endpoint returning trace + predicted rewards

All write operations require Admin role; sandbox allowed for Operator.

---

## Later Steps to Follow
* [ ] Advanced analytics: rule performance, reward issuance trends (read‑only dashboards).
* [ ] Multi‑tenant support (org switcher) if backend supports tenants.
* [ ] Feature flags for editor and import/export to roll out safely.
* [ ] Plugin management UI (list installed condition/reward plugins, schema docs).
* [ ] Webhook management views (endpoints, delivery logs, retry controls) if available.
* [ ] Theming/branding options; i18n.
* [ ] Offline drafts and collaborative editing (CRDT) in the rule editor.

---

## Acceptance and Done Criteria per Phase
- All tasks for the phase implemented with unit tests where applicable.
- E2E happy path for the phase’s core flow.
- Accessibility passes for interactive features (keyboard and screen reader basics).
- No console errors; typecheck passes; CI green.

---

## Notes
- This Admin Panel will be moved into a separate UI repository. The plan intentionally avoids coupling to the C# solution. All integrations occur over REST endpoints that already exist or will be added independently to the API project.



