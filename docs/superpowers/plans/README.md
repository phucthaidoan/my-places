# My Places — Implementation Plans Index

**Spec:** [2026-04-18-mvp-design.md](../specs/2026-04-18-mvp-design.md)

## Plans (thứ tự thực hiện)

| # | Plan | Status | Mô tả |
|---|------|--------|-------|
| 1 | [2026-04-18-01-solution-setup.md](2026-04-18-01-solution-setup.md) | ✅ Done | Solution, EF Core entities, Auth (JWT + Google), Result Pattern |
| 2 | [2026-04-18-02-places-api.md](2026-04-18-02-places-api.md) | ✅ Done | Places API + Blazor Client |
| 3 | [2026-04-18-03-trips-api.md](2026-04-18-03-trips-api.md) | ✅ Done | Trips API + Blazor Client |
| 4 | [2026-04-18-04-e2e-playwright.md](2026-04-18-04-e2e-playwright.md) | ✅ Done | E2E Playwright (repo) + MCP verification |
| 5 | _(chưa viết)_ | ⏳ Pending | Photos (SAS upload flow + client-side resize) |
| 6 | _(chưa viết)_ | ⏳ Pending | Social (Follow, Search Users, Feed) |
| 7 | _(chưa viết)_ | ⏳ Pending | Dashboard + Azure Deploy |

## Dependency Order

```
Plan 1 (Foundation)
    ↓
Plan 2 (Places) ← depends on Plan 1
Plan 3 (Trips)  ← depends on Plan 1 + Plan 2
    ↓
Plan 4 (E2E Playwright) ← depends on Plan 3 (Blazor flows to test)
    ↓
Plan 5 (Photos) ← depends on Plan 2
Plan 6 (Social) ← depends on Plan 1
    ↓
Plan 7 (Dashboard + Deploy) ← depends on Plans 1–3, 5–6 (E2E optional but recommended before release)
```

---

## Next Steps

### 1. GitHub Setup

**Milestones:**
- [x] Tạo Milestone `MVP`
- [x] Tạo Milestone `Phase 2: Collections`
- [x] Tạo Milestone `Phase 3: Social+`

**Issues — Milestone `MVP`:**
- [x] Auth — Đăng ký / đăng nhập (Email + Google OAuth) → #1, #2, #3
- [x] Places — Tạo / sửa / xóa Place (library + in-trip) → #5, #6
- [x] Photos — Upload ảnh max 3 (client-side resize + Azure Blob SAS) → #9
- [x] Photos — Paste external photo URL → #9
- [x] Trips — Tạo / sửa / xóa Trip → #11
- [x] Trips — Thêm / xóa Place vào Trip → #12
- [x] Trips — Share Trip qua public link → #13, #14
- [x] Social — Follow user bằng username hoặc email → #15, #16
- [x] Dashboard — Recent trips + stats + feed preview → #20
- [x] Social — Activity feed từ following list → #17
- [x] Search — Search places (mình + following) → #6
- [x] Search — Filter places theo City → #6
- [x] Search — GPS "places gần tôi" (radius search) → #8
- [x] Places — Copy place từ người khác (SourcePlaceId) → #7
- [x] Deploy — Manual deploy lên Azure → #19

**Issues — Milestone `Phase 2: Collections`:**
- [x] Collections — Curated Lists (tạo, share link, follow) → #21
- [x] Collections — Collaborative Collections → #22
- [x] Infrastructure — Pagination (cursor-based) → #23
- [x] Infrastructure — CI/CD GitHub Actions → #24
- [x] Infrastructure — Terraform IaC → #25
- [x] Search — Full-text search (tsvector) → #26

**Issues — Milestone `Phase 3: Social+`:**
- [x] Discovery — Explore by City → #27
- [x] Social — Notifications → #28
- [x] Mobile — PWA offline support → #29
- [x] Auth — Passkey (WebAuthn) → #30
- [x] Places — Categories / Tags → #31

**Repo setup:**
- [x] Tạo repo `my-places` (public hoặc private)
- [x] Commit spec docs vào `docs/superpowers/`
- [x] Push initial commit

**GitHub — tạo MVP issues (Plan 3–7 + Playwright MVP):** dùng **GitHub MCP** trong Cursor (server `project-0-my-places-github`, ví dụ tool `issue_write`) cùng PAT đã cấu hình; nếu MCP không dùng được thì tạo/cập nhật issue bằng **`gh`** hoặc giao diện GitHub (milestone `MVP`).

---

### 2. Implement Plans

- [x] **Plan 1** — Solution Setup & Core Infrastructure → dùng `superpowers:subagent-driven-development`
- [x] **Plan 2** — Places API + Blazor Client → PR #32
- [x] **Plan 3** — Trips API + Blazor Client → [2026-04-18-03-trips-api.md](2026-04-18-03-trips-api.md)
- [x] **Plan 4** — E2E Playwright + MCP verification → [2026-04-18-04-e2e-playwright.md](2026-04-18-04-e2e-playwright.md) → thư mục [e2e/](../../../e2e/)
- [ ] **Plan 5** — Photos (SAS upload + client-side resize) _(chưa viết plan chi tiết)_
- [ ] **Plan 6** — Social (Follow, Search, Feed) _(chưa viết plan chi tiết)_
- [ ] **Plan 7** — Dashboard + Azure Deploy _(chưa viết plan chi tiết)_

---

### 3. Post-MVP (trigger Phase 2)

- [ ] Deployed to Azure (App Service F1 + Static Web Apps)
- [ ] Used personally for 2+ weeks
- [ ] Successfully shared a Trip link with a friend
- [ ] → Bắt đầu [Phase 2 spec](../specs/2026-04-18-phase2-collections.md)
