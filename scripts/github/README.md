# GitHub — MVP issues

## GitHub MCP (Cursor) — ưu tiên nếu đã bật

Cấu hình PAT + merge `mcpServers.github` và tool **`issue_write`**: xem [docs/superpowers/github/README.md](../../docs/superpowers/github/README.md).

---

`gh` có thể không có trên máy; fallback dưới đây dùng **GitHub CLI** + file body có sẵn.

## One-time setup

1. Install [GitHub CLI](https://cli.github.com/) (`winget install GitHub.cli`).
2. `gh auth login`
3. In the GitHub repo, ensure milestone **MVP** exists (Issues → Milestones → New milestone).

## Create issues

From repository root:

```powershell
pwsh ./scripts/github/New-MvpIssues.ps1
```

Dry run (prints `gh` commands only):

```powershell
pwsh ./scripts/github/New-MvpIssues.ps1 -DryRun
```

Without milestone (if you have not created **MVP** yet):

```powershell
pwsh ./scripts/github/New-MvpIssues.ps1 -Milestone ''
```

## Đồng bộ sau Plan 3 (đóng issue Trips trên GitHub)

Khi code Plan 3 đã xong trong repo, đóng các issue MVP tương ứng trên GitHub (mặc định **#11–#14** — tiêu đề `[Trips] …`).

Từ thư mục gốc repo:

```powershell
pwsh ./scripts/github/Sync-Plan3IssuesComplete.ps1
```

Dry-run (chỉ in việc sẽ làm):

```powershell
pwsh ./scripts/github/Sync-Plan3IssuesComplete.ps1 -DryRun
```

Thêm comment lên issue Plan 4 (**#33**) rằng dependency Plan 3 đã xong:

```powershell
pwsh ./scripts/github/Sync-Plan3IssuesComplete.ps1 -AlsoCommentPlan4Issue
```

**Yêu cầu:** `gh auth login` (khuyến nghị) hoặc biến môi trường `GITHUB_TOKEN` (classic PAT, scope `repo`).

---

## What gets created

| Title | Body file |
|-------|-----------|
| [MVP] Plan 3 — Trips API + Blazor Client | `bodies/mvp-plan3-trips.md` |
| [MVP] Plan 4 — Playwright E2E in repo + MCP verification (MVP) | `bodies/mvp-plan4-playwright.md` |
| [MVP] Plan 4 — Optional: GitHub Actions job for Playwright | `bodies/mvp-plan4-playwright-ci.md` |
| [MVP] Plan 5 — Photos — plan TBD | `bodies/mvp-plan5-photos.md` |
| [MVP] Plan 6 — Social — plan TBD | `bodies/mvp-plan6-social.md` |
| [MVP] Plan 7 — Dashboard + Azure deploy — plan TBD | `bodies/mvp-plan7-dashboard-deploy.md` |

Playwright is explicitly **in MVP scope** in the Plan 4 issue body (suite + MCP runbook; CI issue is optional).

## Playwright issues trên GitHub (`phucthaidoan/my-places`)

Đã tạo bằng GitHub MCP (`issue_write`):

| # | URL |
|---|-----|
| **33** | https://github.com/phucthaidoan/my-places/issues/33 — Playwright E2E in repo + MCP verification (MVP) |
| **34** | https://github.com/phucthaidoan/my-places/issues/34 — Optional: GitHub Actions job for Playwright (issue #34 body tham chiếu #33) |

Gán milestone **MVP** trên GitHub UI nếu cần (MCP `milestone` là số, không phải tên).

Tạo lại bằng `gh` (nếu không dùng MCP):

```powershell
cd c:\Workspaces\Projects\pet\my-places
gh issue create -R phucthaidoan/my-places --title "[MVP] Plan 4 — Playwright E2E in repo + MCP verification (MVP)" --body-file scripts/github/bodies/mvp-plan4-playwright.md
gh issue create -R phucthaidoan/my-places --title "[MVP] Plan 4 — Optional: GitHub Actions job for Playwright" --body-file scripts/github/bodies/mvp-plan4-playwright-ci.md
```

## Manual creation

Copy any file under `bodies/` into a new GitHub issue description and set the title to match the script’s `Title` for that file.
