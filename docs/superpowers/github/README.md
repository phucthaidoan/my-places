# GitHub MCP (Cursor) — tạo issue cho repo `my-places`

Official guide: [Install GitHub MCP Server in Cursor](https://github.com/github/github-mcp-server/blob/main/docs/installation-guides/install-cursor.md).

## Chỉ cho project `my-places` (khuyến nghị)

1. Tạo [Classic PAT](https://github.com/settings/tokens) với scope **`repo`**.
2. Trong **root repo**, dùng đúng tên file: **`my-places/.cursor/mcp.json`** (tên file là `mcp.json`, **không** phải `.mcp.json` — Cursor không load file sai tên).
3. Copy nội dung từ [cursor-mcp-github-snippet.json](cursor-mcp-github-snippet.json), thay `YOUR_GITHUB_PAT` bằng PAT (file này đã được `.gitignore` để tránh commit nhầm).
4. **Restart Cursor**; Settings → MCP: server **`github`** chấm xanh.
5. **Composer / Chat thường** mới có MCP GitHub; nhiều luồng Agent trong IDE **không** mount MCP nên có thể báo `MCP server does not exist: github` dù cấu hình đúng.

## Bước nhanh (global — merge vào profile)

1. Tạo [Classic PAT](https://github.com/settings/tokens) với scope **`repo`** (tạo issue trong repo private cần quyền này).
2. Mở **`%USERPROFILE%\.cursor\mcp.json`** (Windows) và **merge** object `github` từ file [cursor-mcp-github-snippet.json](cursor-mcp-github-snippet.json) vào `mcpServers` hiện có (đừng xóa các server khác như `context7`, `grep`, …).
3. Thay `YOUR_GITHUB_PAT` bằng PAT thật (hoặc dùng UI Cursor: Tools → MCP → github → chỉnh header).
4. **Khởi động lại Cursor** hoàn toàn; kiểm tra MCP `github` có chấm xanh.
5. Trong chat/Agent, yêu cầu dùng tool **`issue_write`** với `method: "create"`, `owner`, `repo`, `title`, `body` (xem [GitHub MCP README — issue_write](https://github.com/github/github-mcp-server/blob/main/README.md)).

## Vì sao agent có thể chưa tạo được issue

Nếu Cursor báo **`MCP server does not exist: github`**, nghĩa là bước 2–4 chưa xong hoặc tên server khác (phải đúng key `"github"` trong `mcpServers` trừ khi bạn đổi tên).

## Đồng bộ trạng thái issue (ví dụ sau Plan 3)

Sau khi merge/triển khai xong trong repo, cập nhật issue trên GitHub (đóng #11–#14, v.v.) bằng script:

- [scripts/github/README.md](../../scripts/github/README.md) — mục **Đồng bộ sau Plan 3**
- Script: `scripts/github/Sync-Plan3IssuesComplete.ps1` (cần `gh` hoặc `GITHUB_TOKEN`)

---

## Milestone `MVP`

GitHub API dùng **số** milestone (`milestone` là number), không phải tên. Có thể:

- Tạo issue **không** gán milestone qua MCP, rồi gán tay trên GitHub, hoặc  
- Gọi API/GitHub UI để lấy `milestone_number` của **MVP** rồi truyền vào `issue_write`.

## Nội dung issue có sẵn trong repo

Tiêu đề + body mẫu nằm trong [scripts/github/bodies](../../scripts/github/bodies/) và script `gh`: [scripts/github/README.md](../../scripts/github/README.md).
