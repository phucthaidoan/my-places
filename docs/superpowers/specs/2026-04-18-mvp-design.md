# My Places — MVP Design Spec
**Date:** 2026-04-18  
**Status:** Draft — Pending User Review  
**Phase:** MVP (Phase 1)

---

## 1. Problem & Goal

Người dùng hay đi du lịch và muốn lưu lại các địa điểm quán ăn ngon để xem lại khi quay lại nơi đó. Ngoài ra, họ muốn chia sẻ danh sách địa điểm cho bạn bè và xem lại địa điểm từ bạn bè đang follow.

**MVP Goal:** Web app cho phép lưu địa điểm quán ăn, tổ chức theo chuyến đi, chia sẻ qua link, và follow bạn bè để xem địa điểm của nhau.

---

## 2. Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WebAssembly (.NET 10 LTS) |
| Backend | ASP.NET Core 10 Minimal API |
| ORM | Entity Framework Core 10 |
| Database | Neon PostgreSQL (serverless, free tier) |
| Storage | Azure Blob Storage (free tier 5GB) |
| Auth | ASP.NET Core Identity + JWT + Google OAuth |
| Hosting — Client | Azure Static Web Apps (free tier) |
| Hosting — API | Azure App Service F1 (free tier, 60 CPU min/day) |
| Architecture | Vertical Slice, Result Pattern |

**.NET 10 notable features used:**
- Built-in validation (`AddValidation()`) — không cần FluentValidation riêng
- `IProblemDetailsService` — consistent error responses, tích hợp Result Pattern
- Smart parameter binding — không cần `[FromBody]`
- Blazor WASM bundle giảm 76% so với .NET 9 (43KB vs 183KB)

---

## 3. Architecture

```
Blazor WASM (browser — Azure Static Web Apps)
    ↕ HTTP/JSON (REST)
ASP.NET Core 10 Minimal API (Azure App Service F1)
    ↕ EF Core 10
Neon PostgreSQL (serverless)
    +
Azure Blob Storage (photos)
```

### Monorepo Solution Structure

```
my-places/
├── src/
│   ├── MyPlaces.Api/                    # ASP.NET Core Minimal API
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   │   ├── Register.cs
│   │   │   │   ├── Login.cs
│   │   │   │   └── GoogleLogin.cs
│   │   │   ├── Places/
│   │   │   │   ├── CreatePlace.cs
│   │   │   │   ├── GetPlaces.cs
│   │   │   │   ├── UpdatePlace.cs
│   │   │   │   ├── DeletePlace.cs
│   │   │   │   ├── CopyPlace.cs
│   │   │   │   └── GetNearbyPlaces.cs
│   │   │   ├── Trips/
│   │   │   │   ├── CreateTrip.cs
│   │   │   │   ├── GetTrips.cs
│   │   │   │   ├── UpdateTrip.cs
│   │   │   │   ├── DeleteTrip.cs
│   │   │   │   ├── AddPlaceToTrip.cs
│   │   │   │   ├── RemovePlaceFromTrip.cs
│   │   │   │   └── ShareTrip.cs
│   │   │   ├── Photos/
│   │   │   │   ├── GetSasToken.cs
│   │   │   │   └── ConfirmPhoto.cs
│   │   │   ├── Social/
│   │   │   │   ├── FollowUser.cs
│   │   │   │   ├── UnfollowUser.cs
│   │   │   │   └── SearchUsers.cs
│   │   │   └── Dashboard/
│   │   │       ├── GetDashboard.cs
│   │   │       └── GetFeed.cs
│   │   ├── Common/
│   │   │   ├── Result.cs
│   │   │   ├── AppDbContext.cs
│   │   │   └── Extensions/
│   │   └── Program.cs
│   │
│   ├── MyPlaces.Client/                 # Blazor WebAssembly
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   ├── Places/
│   │   │   ├── Trips/
│   │   │   ├── Dashboard/
│   │   │   └── Social/
│   │   ├── Shared/                      # Layout, NavMenu, Toast, ErrorBoundary
│   │   └── Program.cs
│   │
│   └── MyPlaces.Shared/                 # DTOs shared giữa API và Client
│       ├── Places/
│       ├── Trips/
│       ├── Auth/
│       └── Common/
│           └── Result.cs
│
├── docs/
│   └── superpowers/specs/
└── MyPlaces.sln
```

### Vertical Slice Pattern

Mỗi feature là một file độc lập chứa Request, Response, Handler, và Endpoint registration:

```csharp
// Features/Places/CreatePlace.cs
public static class CreatePlace
{
    public record Request(string Name, string Address, string City,
        string? Country, double? Latitude, double? Longitude, string? Note);
    public record Response(Guid Id, string Name, string Address, string City);

    public static async Task<IResult> Handle(
        Request req, AppDbContext db, ICurrentUser user, CancellationToken ct)
    {
        var place = new Place { UserId = user.Id, Name = req.Name, ... };
        db.Places.Add(place);
        await db.SaveChangesAsync(ct);
        return Results.Ok(Result<Response>.Success(new Response(place.Id, ...)));
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/v1/places", Handle).RequireAuthorization();
}
```

---

## 4. Data Model

```
User
├── Id (Guid)
├── Email (unique)
├── PasswordHash
├── Username (unique — dùng để tìm kiếm/follow)
├── DisplayName
├── AvatarUrl (nullable)
└── CreatedAt

Place (độc lập, không gắn cứng với Trip)
├── Id (Guid)
├── UserId (owner)
├── Name (tên quán)
├── Address
├── City
├── Country (nullable)
├── Latitude (nullable — từ GPS)
├── Longitude (nullable — từ GPS)
├── Note (ghi chú ngắn, nullable)
├── SourcePlaceId (nullable — copy từ place của người khác)
└── CreatedAt

PlacePhoto (tối đa 3 ảnh/place)
├── Id (Guid)
├── PlaceId
├── PhotoUrl (Azure Blob URL hoặc external URL)
├── IsExternal (bool — phân biệt upload vs paste URL)
└── SortOrder (0-2)

Trip
├── Id (Guid)
├── UserId (owner)
├── Name (e.g. "Đà Nẵng tháng 3")
├── City
├── Country (nullable)
├── StartDate (nullable)
├── EndDate (nullable)
├── Visibility (Private | PublicLink)
├── ShareToken (nullable unique slug — generated khi set PublicLink)
└── CreatedAt

TripPlace (many-to-many: Trip ↔ Place)
├── TripId
├── PlaceId
└── AddedAt

Follow (one-way)
├── FollowerId
├── FollowingId
└── CreatedAt
```

### Database Indexes

```sql
CREATE INDEX idx_places_user_created ON places(user_id, created_at DESC);
CREATE INDEX idx_places_city_user ON places(city, user_id);
CREATE INDEX idx_places_location ON places USING gist(ll_to_earth(latitude, longitude));
CREATE INDEX idx_follow_follower ON follow(follower_id, following_id);
CREATE INDEX idx_tripplace_trip ON trip_place(trip_id, place_id);
CREATE INDEX idx_trips_user ON trips(user_id, created_at DESC);
```

### Place Visibility Rule
Place không có visibility riêng. Khi xem Place qua Trip context → kế thừa visibility của Trip. Place trong library của owner luôn visible với owner.

---

## 5. API Endpoints

```
Auth
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/google
POST   /api/v1/auth/refresh-token
POST   /api/v1/auth/forgot-password

Places
GET    /api/v1/places              # My library + search + filter by city
POST   /api/v1/places              # Tạo place mới (từ library hoặc trong Trip)
GET    /api/v1/places/{id}
PUT    /api/v1/places/{id}
DELETE /api/v1/places/{id}
POST   /api/v1/places/{id}/copy    # Copy place của người khác (ghi SourcePlaceId)
GET    /api/v1/places/nearby       # GPS radius search (query: lat, lng, radiusKm)

Photos
GET    /api/v1/photos/sas-token    # Lấy SAS URL để upload trực tiếp lên Blob
POST   /api/v1/photos/confirm      # Lưu URL vào DB sau khi upload xong
DELETE /api/v1/photos/{id}

Trips
GET    /api/v1/trips               # My trips
POST   /api/v1/trips               # Tạo trip mới
GET    /api/v1/trips/{id}
PUT    /api/v1/trips/{id}
DELETE /api/v1/trips/{id}
POST   /api/v1/trips/{id}/places/{placeId}    # Thêm place vào trip
DELETE /api/v1/trips/{id}/places/{placeId}    # Xóa place khỏi trip
POST   /api/v1/trips/{id}/share               # Generate/toggle share token
GET    /api/v1/trips/shared/{token}           # Public — xem trip (no auth required)

Social
GET    /api/v1/users/search        # Tìm user theo username hoặc email
POST   /api/v1/users/{id}/follow
DELETE /api/v1/users/{id}/follow
GET    /api/v1/users/{id}/trips    # Xem public trips của user đang follow

Dashboard
GET    /api/v1/dashboard           # Recent trips + stats của mình
GET    /api/v1/feed                # Activity feed của người mình follow (places + trips mới)
```

---

## 6. Authentication Flow

```
Email/Password:
  POST /auth/register → tạo user, trả JWT + refresh token
  POST /auth/login → xác thực, trả JWT + refresh token
  POST /auth/refresh-token → đổi refresh token mới

Google OAuth:
  POST /auth/google { idToken } → validate Google token
    → tạo user nếu chưa có (email từ Google profile)
    → trả JWT + refresh token

JWT: short-lived (15 phút)
Refresh Token: long-lived (7 ngày), lưu HttpOnly cookie
```

---

## 7. Image Upload Flow

```
1. Client resize ảnh xuống max 1200px width, JPEG quality 80% (~200-400KB)
   (dùng Canvas API trong Blazor WASM qua JS Interop)
2. GET /api/v1/photos/sas-token → nhận Azure Blob SAS URL (expiry 5 phút)
3. PUT {sasUrl} — upload trực tiếp lên Azure Blob (không qua API server)
4. POST /api/v1/photos/confirm { placeId, blobUrl } → lưu PlacePhoto vào DB
5. Giới hạn: tối đa 3 ảnh/place, validate SortOrder khi confirm
```

---

## 8. Location — City & GPS

**Tạo Place trong Trip context:** City tự động điền từ Trip.City, user có thể override.

**Tạo Place từ library (không trong Trip):**
- GPS: browser Geolocation API → reverse geocoding (Azure Maps free tier: 5,000 req/month) → tự động điền City
- Hoặc user nhập tay

**Filter Places theo City:** `ILIKE '%đà nẵng%'` (case-insensitive, PostgreSQL).

**GPS Radius Search:** Haversine formula qua EF Core raw SQL hoặc PostGIS extension trên Neon.

---

## 9. Search

**Scope:** Places của mình + Places của người mình follow (chỉ public places của họ, thông qua Trip visibility).

**Implementation:** PostgreSQL `ILIKE` trên `name`, `address`, `note` fields. Full-text search (`tsvector`) có thể thêm Phase 2 nếu cần.

---

## 10. Dashboard (Home Screen)

```json
GET /api/v1/dashboard trả về:
{
  "recentTrips": [...],        // 5 trips gần nhất của mình
  "myStats": {
    "totalPlaces": 42,
    "totalTrips": 8,
    "cities": 12
  },
  "feedPreview": [...]         // 5 items mới nhất từ following feed
}
```

Feed query:
```csharp
db.Places
  .Where(p => db.Follows
    .Where(f => f.FollowerId == currentUserId)
    .Select(f => f.FollowingId)
    .Contains(p.UserId))
  .OrderByDescending(p => p.CreatedAt)
  .Take(20)
```

---

## 11. Error Handling

**Result Pattern:**
```csharp
public record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

**HTTP Error mapping (IProblemDetailsService):**
| Status | Scenario |
|--------|---------|
| 400 | Validation failed |
| 401 | Unauthenticated → redirect login |
| 403 | Forbidden (not owner) |
| 404 | Resource not found |
| 500 | Unexpected server error |

**Offline behavior (Blazor WASM):**
- Mất mạng khi browsing → toast "Không có kết nối mạng", data đã load giữ nguyên
- Mất mạng khi đang tạo/edit Place → form giữ state, nút Save hiển thị error + retry
- Không có offline cache (PWA deferred to Phase 3)

---

## 12. MVP Scope

### IN — có trong MVP
- Đăng ký / đăng nhập (Email + Google OAuth)
- Tạo / sửa / xóa Place (từ library hoặc trong Trip)
- Upload ảnh tối đa 3 (resize client-side + Azure Blob SAS)
- Paste external photo URL
- Tạo / sửa / xóa Trip
- Thêm / xóa Place vào Trip
- Share Trip qua public link
- Follow user bằng username hoặc email
- Dashboard: recent trips + stats + feed preview
- Activity feed từ following list
- Search places (của mình + following)
- Filter places theo City
- GPS "places gần tôi" (radius search)
- Copy place từ người khác (ghi SourcePlaceId)
- Manual deploy lên Azure

### OUT — không có trong MVP (Phase 2+)
| Feature | Phase |
|---------|-------|
| Collections (Curated Lists) | 2 |
| Collaborative Collections | 2 |
| Pagination | 2 |
| CI/CD tự động (GitHub Actions) | 2 |
| Terraform IaC | 2 |
| Discovery / Explore page | 3 |
| Notifications | 3 |
| Mobile PWA / Offline | 3 |
| Full-text search (tsvector) | 2 |

---

## 13. Deployment (Manual — MVP)

```bash
# Blazor WASM → Azure Static Web Apps
dotnet publish src/MyPlaces.Client -c Release
az staticwebapp deploy --app-location src/MyPlaces.Client/bin/Release/net10.0/publish/wwwroot

# API → Azure App Service F1
dotnet publish src/MyPlaces.Api -c Release
az webapp deploy --resource-group my-places-rg --name my-places-api \
  --src-path src/MyPlaces.Api/bin/Release/net10.0/publish
```

**Azure resources cần tạo:**
- Resource Group: `my-places-rg`
- Azure Static Web Apps (free)
- Azure App Service F1 (free)
- Azure Blob Storage (free 5GB)
- Neon PostgreSQL (free 0.5GB)

---

## 14. GitHub Setup (sau khi spec approved)

1. Tạo repo `my-places`
2. Tạo Milestones: `MVP` | `Phase 2: Collections` | `Phase 3: Social`
3. Tạo Issues từ MVP IN list → gán Milestone `MVP`
4. Commit spec vào `docs/superpowers/specs/`
5. Tạo initial solution structure

---

## 15. Phase 2+ Backlog

| Phase | Feature |
|-------|---------|
| 2 | Collections — themed lists, share link, follow collection |
| 2 | Collaborative Collections |
| 2 | Pagination (cursor-based) |
| 2 | CI/CD — GitHub Actions auto deploy |
| 2 | Terraform IaC cho Azure resources |
| 2 | Full-text search (PostgreSQL tsvector) |
| 3 | Discovery / Explore theo City |
| 3 | Notifications (khi following thêm Place/Trip mới) |
| 3 | Mobile PWA — offline support, install to homescreen |
| 3 | Passkey authentication (WebAuthn — .NET 10 built-in) |

---

## MVP Done Criteria

- [ ] Deployed lên Azure (Static Web Apps + App Service)
- [ ] Bản thân dùng được ít nhất 2 tuần
- [ ] Đã share thành công Trip link cho bạn bè
