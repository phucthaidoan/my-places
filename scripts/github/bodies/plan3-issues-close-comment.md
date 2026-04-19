Đóng issue: **Plan 3 — Trips API + Blazor Client** đã triển khai trong repo.

- Plan: `docs/superpowers/plans/2026-04-18-03-trips-api.md` (checklist đã đồng bộ).
- API: CRUD trips, thêm/xóa place trong trip, share toggle + `GET /api/v1/trips/shared/{token}` (anonymous), integration tests.
- Client: `ApiBaseUrl`, JWT (localStorage + `DelegatingHandler`), Login/Register, Trips list/editor/detail/shared view.
- Places (prerequisite Plan 2): `DELETE`, `POST …/copy`, `GET …/nearby` đã đăng ký đúng thứ tự.
- Auth: `PostConfigure<AuthenticationOptions>` để **JWT Bearer** là scheme mặc định (Identity không còn override cookie).

Nếu cần rà soát PR/commit, xem lịch sử merge gần nhất trên nhánh chính / PR liên quan Plan 3.
