# E2E tests — Playwright (repo) + Playwright MCP (agent verification)

> **For agentic workers:** Use `superpowers:subagent-driven-development` or `superpowers:executing-plans` task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Add a **small, committed** Playwright suite that runs against the real Blazor WASM client + ASP.NET API stack, covering auth and Trips flows (including anonymous shared trip). Document how **Playwright MCP** in Cursor complements this for interactive debugging and pre-merge smoke — without replacing `dotnet test` or the checked-in specs.

**Prerequisite:** [Plan 3 — Trips API + Blazor Client](2026-04-18-03-trips-api.md) implemented enough that: login/register works, `/trips` and shared trip URL work, API and client dev URLs are stable (document in this plan).

**Non-goals:** Full visual regression suite; testing every API edge case (keep those in `MyPlaces.Api.Tests`).

---

## Rationale

| Layer | Tool |
|-------|------|
| API contracts, ownership, share rules | `dotnet test` + Testcontainers |
| CORS, JWT attachment, routing, WASM load | Playwright E2E |
| Agent “did it work in a browser?” before claiming done | Playwright **MCP** + same `baseURL` as local E2E |

---

## File Map (suggested)

```
my-places/
├── e2e/
│   ├── package.json              # @playwright/test, scripts: test, test:ui
│   ├── playwright.config.ts      # baseURL, webServer, retries, trace on failure
│   ├── fixtures/
│   │   └── auth.ts               # optional: save storageState after register/login
│   └── specs/
│       ├── smoke.spec.ts         # app loads, health
│       ├── auth.spec.ts          # register + login (or login-only if seeded)
│       ├── trips.spec.ts         # create trip, add place, share, assert link
│       └── shared-trip.spec.ts   # open /shared/{token} without auth context
├── docs/superpowers/plans/
│   └── 2026-04-18-04-e2e-playwright.md   # (this file)
```

**Optional CI:** `.github/workflows/e2e.yml` — install Node, `npx playwright install --with-deps`, start API + Client (or use `webServer` in config), run `npm test` in `e2e/`.

---

## Configuration conventions

- **`baseURL`:** Blazor dev URL (e.g. `http://localhost:5xxx`) from env `E2E_BASE_URL` with default in `playwright.config.ts`.
- **API:** Client `appsettings.Development.json` should point API to a running instance; E2E assumes developer runs API + Client **or** `webServer` starts both (harder on one machine — document two-process vs `dotnet run` profiles).
- **Data:** Prefer **unique emails** per test run (`test+${Date.now()}@example.com`) to avoid collisions unless tests reset DB (not required for MVP E2E if isolation is per email).

---

## Task 1: Scaffold `e2e/` project

- [x] `cd e2e && npm init -y` (or pnpm) and add `@playwright/test`.

- [x] `npx playwright install` (document Chromium-only for CI to save time).

- [x] Add `playwright.config.ts` with `testDir: './specs'`, reasonable `timeout`, `trace: 'on-first-retry'`.

- [x] Add `.gitignore` under `e2e/` for `test-results/`, `playwright-report/`, `blob-report/`.

- [x] Root or `e2e/README.md` one paragraph: how to run (`E2E_BASE_URL=... npm test`), prerequisite “API + Client running”.

- [ ] Commit: `chore(e2e): add Playwright scaffold`

---

## Task 2: Smoke spec

- [x] `smoke.spec.ts`: `page.goto('/')` (or `/login`) — expect HTTP 200 and visible shell (title, nav, or login form).

- [ ] Commit: `test(e2e): smoke spec`

---

## Task 3: Auth spec

- [x] Flow: register new user → assert redirect or success UI → login (if separate) → assert authenticated marker (e.g. nav shows Trips).

- [x] Use stable `data-testid` on critical buttons/inputs in Blazor (Plan 3 / follow-up) — prefer testids over fragile CSS.

- [ ] Commit: `test(e2e): auth register and login`

---

## Task 4: Trips + shared link spec

- [x] Logged-in: create a place (if UI exists) or use API from `request` fixture with saved token — **prefer UI** if Plan 3 includes place creation; otherwise document “seed via API” using `API.request` in Playwright.

- [x] Create trip, add place to trip, invoke share, read `SharePath` / URL from UI.

- [x] New browser context **without** storage: open shared URL, assert trip name and at least one place visible.

- [ ] Commit: `test(e2e): trip share flow and anonymous shared view`

---

## Task 5: Playwright MCP (documentation only in repo)

- [x] In `e2e/README.md` (or this plan §Verification): short list of MCP-driven checks agents should run before claiming Plan 3/4 done (open shared link, screenshot on failure).

- [x] No MCP config in-repo required (lives in Cursor user MCP settings).

- [ ] Commit: `docs(e2e): document Playwright MCP verification`

---

## Task 6: CI (optional but recommended)

- [x] GitHub Action job: services or steps to start Postgres if API self-hosts in CI; **simplest path** for MVP: job `workflow_dispatch` only, or skip CI until deploy story exists. _(Không thêm workflow; PR chạy E2E local — `e2e/README.md`.)_

- [x] If full CI is deferred, document “local + PR manual” in README.

- [ ] Commit (if implemented): `ci: add optional e2e workflow`

---

## Verification

1. With API + Client running locally: `cd e2e && npx playwright test` — all green.

2. `dotnet test` still passes unchanged.

3. Agent checklist: run at least **smoke + shared trip** via Playwright MCP after UI changes.

---

## Relation to later plans

- **Plan 5 (Photos):** extend `trips.spec.ts` or add `photos.spec.ts` when upload UI exists.
- **Plan 6–7:** add feed/dashboard specs incrementally; keep suite small.
