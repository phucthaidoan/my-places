# My Places — Playwright E2E

End-to-end tests against the Blazor WASM client (`E2E_BASE_URL`, default `http://localhost:5178`) and the ASP.NET API (`E2E_API_BASE_URL`, default `http://localhost:5160`). The API must match the client’s `wwwroot/appsettings.Development.json` `ApiBaseUrl`.

## Prerequisites

1. **PostgreSQL** — from repo root: `docker compose up -d` (port `5434`, see `docker-compose.yml` and `src/MyPlaces.Api/appsettings.Development.json`).
2. **API** — `dotnet run --project src/MyPlaces.Api` (default `http://localhost:5160`).
3. **Client** — `dotnet run --project src/MyPlaces.Client` (default `http://localhost:5178`).
4. **Browsers** — once: `cd e2e && npx playwright install chromium` (use `--with-deps` on Linux CI if you run tests there yourself).

## Run

```bash
cd e2e
npm install
npx playwright test
```

Optional: `npx playwright test --ui` for the Playwright UI, or set `E2E_BASE_URL` / `E2E_API_BASE_URL` if your dev ports differ.

## Before opening a PR

Run the full stack locally, then `cd e2e && npx playwright test` and ensure all tests pass. This repo does **not** run Playwright on GitHub Actions; reviewers rely on authors running E2E locally.

## Playwright MCP (Cursor)

For quick manual verification after UI changes (complements this suite, does not replace it):

- Open the app at the same `E2E_BASE_URL` as above.
- Run smoke: load `/login`, confirm the Login heading and form.
- After a share flow, open the shared URL in a clean session and confirm trip title and places list.
- On failures, capture a screenshot or trace for the PR description.

MCP server configuration stays in your Cursor user settings, not in this repository.

## Relation to `dotnet test`

API contract and ownership rules remain covered by `MyPlaces.Api.Tests` (Testcontainers). These specs focus on browser routing, auth storage, and trip/share UX.
