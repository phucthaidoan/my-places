# My Places — Phase 2: Collections & Infrastructure

**Date:** 2026-04-18  
**Status:** Rough notes — implement sau khi MVP done criteria đạt được  
**Phase:** 2

---

## MVP Done Criteria (trigger Phase 2)

- [ ] Deployed to Azure (App Service F1 + Static Web Apps)
- [ ] Used personally for 2+ weeks
- [ ] Successfully shared a Trip link with a friend

---

## 1. Collections (Curated Lists)

**Problem:** Người dùng muốn tạo danh sách địa điểm theo chủ đề (không phải theo chuyến đi), ví dụ: "Quán cà phê yêu thích TP.HCM", "Best Ramen Tokyo".

**Core features:**
- Tạo Collection với tên + mô tả + cover photo
- Thêm/xóa Place vào Collection (many-to-many)
- Visibility: Private | PublicLink (giống Trip)
- Share Collection qua link (ShareToken)
- Follow một Collection của người khác

**Data model (rough):**
```
Collection { Id, UserId, Name, Description, CoverPhotoUrl, Visibility, ShareToken, CreatedAt }
CollectionPlace { CollectionId, PlaceId, AddedAt }
CollectionFollow { FollowerId, CollectionId, CreatedAt }
```

**API endpoints (rough):**
- `POST /api/v1/collections` — tạo collection
- `GET /api/v1/collections` — list của mình
- `GET /api/v1/collections/{id}` — detail
- `PUT /api/v1/collections/{id}` — update
- `DELETE /api/v1/collections/{id}` — xóa
- `POST /api/v1/collections/{id}/places` — thêm place
- `DELETE /api/v1/collections/{id}/places/{placeId}` — xóa place
- `GET /api/v1/collections/shared/{token}` — xem collection qua link
- `POST /api/v1/collections/{id}/follow` — follow collection
- `DELETE /api/v1/collections/{id}/follow` — unfollow

**Questions to resolve when writing Phase 2 plan:**
- Collection có xuất hiện trong Feed không?
- Khi follow Collection, activity của collection đó có vào Feed không?
- Copy place từ Collection có dùng SourcePlaceId không? (giống Trip — có thể)

---

## 2. Collaborative Collections

**Problem:** Nhiều người cùng đóng góp địa điểm vào một Collection (du lịch nhóm).

**Core features:**
- Mời collaborator vào Collection qua email/username
- Collaborator có thể thêm/xóa Place (không thể xóa Collection)
- Owner có thể kick collaborator
- Activity log: ai thêm Place nào

**Data model (rough):**
```
CollectionCollaborator { CollectionId, UserId, Role (Owner|Editor), JoinedAt }
```

**Open questions:**
- Notifications khi collaborator thêm Place? (liên quan Phase 3)
- Có cần approve trước khi join không?

---

## 3. Pagination

**Problem:** Khi dữ liệu lớn, load tất cả Places/Trips một lúc sẽ chậm.

**Approach:** Cursor-based pagination (hiệu quả hơn offset cho data append-mostly).

```
GET /api/v1/places?cursor=<createdAt_base64>&limit=20
Response: { items: [...], nextCursor: "..." }
```

**Scope:**
- Places list (by user, by trip, by collection)
- Feed

**Note:** MVP dùng `.Take(50)` hard limit — không cần pagination khi data còn nhỏ.

---

## 4. CI/CD — GitHub Actions

**Problem:** Manual deploy tốn thời gian, dễ quên bước.

**Approach:**
- Push to `main` → trigger GitHub Actions
- Build + test → deploy API lên Azure App Service
- Build Blazor WASM → deploy lên Azure Static Web Apps
- Secrets lưu trong GitHub Secrets

**Rough workflow:**
```yaml
on:
  push:
    branches: [main]
jobs:
  deploy-api:
    runs-on: ubuntu-latest
    steps:
      - dotnet build + test
      - dotnet publish
      - az webapp deploy
  deploy-client:
    runs-on: ubuntu-latest
    steps:
      - dotnet publish
      - az staticwebapp deploy
```

---

## 5. Terraform IaC

**Problem:** Tạo Azure resources thủ công khó reproduce, khó track changes.

**Approach:** Terraform quản lý tất cả Azure resources.

**Resources:**
- `azurerm_resource_group`
- `azurerm_static_web_app`
- `azurerm_service_plan` (F1)
- `azurerm_linux_web_app`
- `azurerm_storage_account` + `azurerm_storage_container`

**Note:** State lưu trong Azure Blob Storage backend.

---

## 6. Full-text Search

**Problem:** `ILIKE` search không hiệu quả với dữ liệu lớn, không support ranking.

**Approach:** PostgreSQL `tsvector` + `tsquery`.

```sql
ALTER TABLE places ADD COLUMN search_vector tsvector
  GENERATED ALWAYS AS (
    to_tsvector('simple', coalesce(name,'') || ' ' || coalesce(address,'') || ' ' || coalesce(note,''))
  ) STORED;
CREATE INDEX idx_places_search ON places USING gin(search_vector);
```

**Note:** MVP dùng ILIKE đủ dùng. Migrate sang tsvector khi cần.

---

## GitHub Milestone

Tạo Milestone `Phase 2: Collections` trên GitHub. Issues sẽ tạo khi bắt đầu Phase 2.
