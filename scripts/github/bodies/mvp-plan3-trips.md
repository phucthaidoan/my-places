## Summary

Implement **Plan 3 — Trips API + Blazor Client** per the superpowers plan.

## Plan document

- `docs/superpowers/plans/2026-04-18-03-trips-api.md`

## MVP spec

- Trips CRUD, add/remove places (owner only), share token + public `GET /trips/shared/{token}`, Blazor UI (list / edit / detail / shared view), minimal auth plumbing for the client.

## Prerequisites

- Confirm `EndpointExtensions` registers **all** Places routes from Plan 2 (including delete, copy, nearby) if not already present — needed for “add place to trip” UX.

## Agent workflow

- Prefer `superpowers:subagent-driven-development` (or `executing-plans`) and check off tasks in the plan file.

## Acceptance

- [x] All Plan 3 tasks in the plan doc completed (or consciously deferred with follow-up issue).
- [x] `dotnet test` passes (API suite; factory dùng InMemory — Docker optional).
- [ ] Manual: create trip, add place, share link, open shared URL without auth _(recommended before release)_.
