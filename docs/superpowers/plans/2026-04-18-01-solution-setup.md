# My Places — Plan 1: Solution Setup & Core Infrastructure

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tạo monorepo solution với 3 projects, EF Core entities, migrations, ASP.NET Core Identity, JWT auth (Email + Google OAuth), và Result Pattern — foundation cho tất cả features sau.

**Architecture:** Vertical Slice monorepo với MyPlaces.Api (Minimal API), MyPlaces.Client (Blazor WASM), MyPlaces.Shared (DTOs). API dùng EF Core + Neon PostgreSQL. Auth dùng ASP.NET Core Identity + JWT + Google OAuth. Result<T> pattern xử lý tất cả responses.

**Tech Stack:** .NET 10 LTS, ASP.NET Core 10 Minimal API, Blazor WebAssembly, EF Core 10, Neon PostgreSQL, ASP.NET Core Identity, JWT Bearer, Google OAuth 2.0

---

## File Map

```
my-places/
├── docker-compose.yml               # Local dev PostgreSQL + pgAdmin
├── MyPlaces.sln
├── src/
│   ├── MyPlaces.Api/
│   │   ├── MyPlaces.Api.csproj
│   │   ├── Program.cs
│   │   ├── Common/
│   │   │   ├── Result.cs                    # Result<T> pattern
│   │   │   ├── AppDbContext.cs              # EF Core DbContext
│   │   │   ├── CurrentUser.cs              # ICurrentUser interface + impl
│   │   │   └── Extensions/
│   │   │       ├── AuthExtensions.cs        # AddJwtAuth(), AddGoogleAuth()
│   │   │       └── EndpointExtensions.cs    # MapAllEndpoints()
│   │   ├── Features/
│   │   │   └── Auth/
│   │   │       ├── Register.cs
│   │   │       ├── Login.cs
│   │   │       ├── GoogleLogin.cs
│   │   │       └── RefreshToken.cs
│   │   └── Migrations/                      # EF Core migrations (auto-generated)
│   │
│   ├── MyPlaces.Client/
│   │   ├── MyPlaces.Client.csproj
│   │   ├── Program.cs
│   │   ├── wwwroot/
│   │   │   └── index.html
│   │   ├── Shared/
│   │   │   ├── MainLayout.razor
│   │   │   └── NavMenu.razor
│   │   └── Features/
│   │       └── Auth/
│   │           ├── Login.razor
│   │           ├── Register.razor
│   │           └── AuthService.cs           # HTTP client wrapper cho auth API
│   │
│   └── MyPlaces.Shared/
│       ├── MyPlaces.Shared.csproj
│       ├── Common/
│       │   └── Result.cs                    # Shared Result<T> (dùng ở Client)
│       └── Auth/
│           ├── RegisterRequest.cs
│           ├── LoginRequest.cs
│           ├── GoogleLoginRequest.cs
│           └── AuthResponse.cs              # { Token, RefreshToken, ExpiresAt }
│
└── tests/
    └── MyPlaces.Api.Tests/
        ├── MyPlaces.Api.Tests.csproj
        └── Features/
            └── Auth/
                ├── RegisterTests.cs
                └── LoginTests.cs
```

---

## Task 0: Docker Local Dev Setup

**Files:**
- Create: `docker-compose.yml`
- Modify: `.gitignore`

Docker chỉ dùng cho local development. Deploy vẫn dùng Neon PostgreSQL + Azure App Service F1.

- [ ] **Step 1: Tạo docker-compose.yml**

Tạo file `docker-compose.yml` ở root project:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: myplaces-postgres
    environment:
      POSTGRES_DB: myplaces_dev
      POSTGRES_USER: myplaces
      POSTGRES_PASSWORD: myplaces_dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: myplaces-pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@myplaces.local
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - postgres

volumes:
  postgres_data:
```

- [ ] **Step 2: Thêm docker volumes vào .gitignore**

Mở `.gitignore` và thêm:

```
# Docker
docker-compose.override.yml
```

- [ ] **Step 3: Khởi động Docker containers**

```bash
docker compose up -d
```

Expected output:
```
✔ Container myplaces-postgres  Started
✔ Container myplaces-pgadmin   Started
```

- [ ] **Step 4: Verify PostgreSQL chạy**

```bash
docker compose ps
```

Expected: Cả hai container status `running`.

- [ ] **Step 5: Update connection string cho local dev**

Connection string dùng cho local (sẽ dùng ở Task 6):
```
Host=localhost;Port=5432;Database=myplaces_dev;Username=myplaces;Password=myplaces_dev_password
```

pgAdmin có thể truy cập tại `http://localhost:5050` (email: `admin@myplaces.local`, password: `admin`).

- [ ] **Step 6: Commit**

```bash
git add docker-compose.yml .gitignore
git commit -m "chore: add Docker Compose for local PostgreSQL dev environment"
```

---

## Task 1: Tạo Solution và 3 Projects

**Files:**
- Create: `MyPlaces.sln`
- Create: `src/MyPlaces.Api/MyPlaces.Api.csproj`
- Create: `src/MyPlaces.Client/MyPlaces.Client.csproj`
- Create: `src/MyPlaces.Shared/MyPlaces.Shared.csproj`
- Create: `tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj`

- [ ] **Step 1: Tạo solution và projects**

```bash
cd c:/Workspaces/Projects/pet/my-places
dotnet new sln -n MyPlaces
dotnet new webapi -n MyPlaces.Api -o src/MyPlaces.Api --framework net10.0 --use-minimal-apis
dotnet new blazorwasm -n MyPlaces.Client -o src/MyPlaces.Client --framework net10.0
dotnet new classlib -n MyPlaces.Shared -o src/MyPlaces.Shared --framework net10.0
dotnet new xunit -n MyPlaces.Api.Tests -o tests/MyPlaces.Api.Tests --framework net10.0
```

- [ ] **Step 2: Thêm projects vào solution**

```bash
dotnet sln add src/MyPlaces.Api/MyPlaces.Api.csproj
dotnet sln add src/MyPlaces.Client/MyPlaces.Client.csproj
dotnet sln add src/MyPlaces.Shared/MyPlaces.Shared.csproj
dotnet sln add tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj
```

- [ ] **Step 3: Thêm project references**

```bash
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj reference src/MyPlaces.Shared/MyPlaces.Shared.csproj
dotnet add src/MyPlaces.Client/MyPlaces.Client.csproj reference src/MyPlaces.Shared/MyPlaces.Shared.csproj
dotnet add tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj reference src/MyPlaces.Api/MyPlaces.Api.csproj
```

- [ ] **Step 4: Thêm NuGet packages cho Api**

```bash
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Google.Apis.Auth
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package Microsoft.IdentityModel.Tokens
dotnet add src/MyPlaces.Api/MyPlaces.Api.csproj package System.IdentityModel.Tokens.Jwt
```

- [ ] **Step 5: Thêm NuGet packages cho Tests**

```bash
dotnet add tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/MyPlaces.Api.Tests/MyPlaces.Api.Tests.csproj package FluentAssertions
```

- [ ] **Step 6: Verify build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git init
git add .
git commit -m "chore: initialize solution with Api, Client, Shared, Tests projects"
```

---

## Task 2: Shared — DTOs và Result Pattern

**Files:**
- Create: `src/MyPlaces.Shared/Common/Result.cs`
- Create: `src/MyPlaces.Shared/Auth/RegisterRequest.cs`
- Create: `src/MyPlaces.Shared/Auth/LoginRequest.cs`
- Create: `src/MyPlaces.Shared/Auth/GoogleLoginRequest.cs`
- Create: `src/MyPlaces.Shared/Auth/AuthResponse.cs`

- [ ] **Step 1: Xóa file mặc định không cần thiết**

```bash
rm src/MyPlaces.Shared/Class1.cs
```

- [ ] **Step 2: Tạo Result<T>**

Tạo file `src/MyPlaces.Shared/Common/Result.cs`:

```csharp
namespace MyPlaces.Shared.Common;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

- [ ] **Step 3: Tạo Auth DTOs**

Tạo `src/MyPlaces.Shared/Auth/RegisterRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3)] string Username,
    [Required, MinLength(6)] string Password,
    [Required] string DisplayName
);
```

Tạo `src/MyPlaces.Shared/Auth/LoginRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
```

Tạo `src/MyPlaces.Shared/Auth/GoogleLoginRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Auth;

public record GoogleLoginRequest(
    [Required] string IdToken
);
```

Tạo `src/MyPlaces.Shared/Auth/AuthResponse.cs`:

```csharp
namespace MyPlaces.Shared.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string Username,
    string DisplayName
);
```

- [ ] **Step 4: Build Shared project**

```bash
dotnet build src/MyPlaces.Shared/MyPlaces.Shared.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/MyPlaces.Shared/
git commit -m "feat: add shared DTOs and Result<T> pattern"
```

---

## Task 3: Api — Entities và AppDbContext

**Files:**
- Create: `src/MyPlaces.Api/Common/Entities/AppUser.cs`
- Create: `src/MyPlaces.Api/Common/Entities/Place.cs`
- Create: `src/MyPlaces.Api/Common/Entities/PlacePhoto.cs`
- Create: `src/MyPlaces.Api/Common/Entities/Trip.cs`
- Create: `src/MyPlaces.Api/Common/Entities/TripPlace.cs`
- Create: `src/MyPlaces.Api/Common/Entities/Follow.cs`
- Create: `src/MyPlaces.Api/Common/AppDbContext.cs`

- [ ] **Step 1: Tạo AppUser entity**

Tạo `src/MyPlaces.Api/Common/Entities/AppUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace MyPlaces.Api.Common.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Place> Places { get; set; } = [];
    public ICollection<Trip> Trips { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
}
```

- [ ] **Step 2: Tạo Place entity**

Tạo `src/MyPlaces.Api/Common/Entities/Place.cs`:

```csharp
namespace MyPlaces.Api.Common.Entities;

public class Place
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Note { get; set; }
    public Guid? SourcePlaceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;
    public ICollection<PlacePhoto> Photos { get; set; } = [];
    public ICollection<TripPlace> TripPlaces { get; set; } = [];
}
```

- [ ] **Step 3: Tạo PlacePhoto entity**

Tạo `src/MyPlaces.Api/Common/Entities/PlacePhoto.cs`:

```csharp
namespace MyPlaces.Api.Common.Entities;

public class PlacePhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlaceId { get; set; }
    public string PhotoUrl { get; set; } = default!;
    public bool IsExternal { get; set; }
    public int SortOrder { get; set; }

    public Place Place { get; set; } = default!;
}
```

- [ ] **Step 4: Tạo Trip entity**

Tạo `src/MyPlaces.Api/Common/Entities/Trip.cs`:

```csharp
namespace MyPlaces.Api.Common.Entities;

public enum TripVisibility { Private, PublicLink }

public class Trip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? Country { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TripVisibility Visibility { get; set; } = TripVisibility.Private;
    public string? ShareToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;
    public ICollection<TripPlace> TripPlaces { get; set; } = [];
}
```

- [ ] **Step 5: Tạo TripPlace entity**

Tạo `src/MyPlaces.Api/Common/Entities/TripPlace.cs`:

```csharp
namespace MyPlaces.Api.Common.Entities;

public class TripPlace
{
    public Guid TripId { get; set; }
    public Guid PlaceId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = default!;
    public Place Place { get; set; } = default!;
}
```

- [ ] **Step 6: Tạo Follow entity**

Tạo `src/MyPlaces.Api/Common/Entities/Follow.cs`:

```csharp
namespace MyPlaces.Api.Common.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Follower { get; set; } = default!;
    public AppUser Following { get; set; } = default!;
}
```

- [ ] **Step 7: Tạo AppDbContext**

Tạo `src/MyPlaces.Api/Common/AppDbContext.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Place> Places => Set<Place>();
    public DbSet<PlacePhoto> PlacePhotos => Set<PlacePhoto>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripPlace> TripPlaces => Set<TripPlace>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TripPlace>()
            .HasKey(tp => new { tp.TripId, tp.PlaceId });

        builder.Entity<Follow>()
            .HasKey(f => new { f.FollowerId, f.FollowingId });

        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Follow>()
            .HasOne(f => f.FollowingNav)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.Entity<Place>()
            .HasIndex(p => new { p.UserId, p.CreatedAt });
        builder.Entity<Place>()
            .HasIndex(p => new { p.City, p.UserId });
        builder.Entity<Trip>()
            .HasIndex(t => new { t.UserId, t.CreatedAt });
        builder.Entity<Trip>()
            .HasIndex(t => t.ShareToken)
            .IsUnique()
            .HasFilter("share_token IS NOT NULL");
        builder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId });
        builder.Entity<PlacePhoto>()
            .HasIndex(pp => new { pp.PlaceId, pp.SortOrder });
    }
}
```

**Lưu ý:** Cần fix `Follow` entity để có navigation property đúng tên:

Mở lại `src/MyPlaces.Api/Common/Entities/Follow.cs` và update:

```csharp
namespace MyPlaces.Api.Common.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Follower { get; set; } = default!;
    public AppUser FollowingNav { get; set; } = default!;
}
```

- [ ] **Step 8: Build**

```bash
dotnet build src/MyPlaces.Api/MyPlaces.Api.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 9: Commit**

```bash
git add src/MyPlaces.Api/Common/
git commit -m "feat: add EF Core entities and AppDbContext"
```

---

## Task 4: Api — ICurrentUser và Extensions

**Files:**
- Create: `src/MyPlaces.Api/Common/CurrentUser.cs`
- Create: `src/MyPlaces.Api/Common/Extensions/AuthExtensions.cs`
- Create: `src/MyPlaces.Api/Common/Extensions/EndpointExtensions.cs`

- [ ] **Step 1: Tạo ICurrentUser**

Tạo `src/MyPlaces.Api/Common/CurrentUser.cs`:

```csharp
using System.Security.Claims;

namespace MyPlaces.Api.Common;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => Guid.Parse(
        accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("User not authenticated"));

    public string Email => accessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email)
        ?? throw new InvalidOperationException("User not authenticated");
}
```

- [ ] **Step 2: Tạo AuthExtensions**

Tạo `src/MyPlaces.Api/Common/Extensions/AuthExtensions.cs`:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MyPlaces.Api.Common.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services, IConfiguration config)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is required");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }
}
```

- [ ] **Step 3: Tạo EndpointExtensions**

Tạo `src/MyPlaces.Api/Common/Extensions/EndpointExtensions.cs`:

```csharp
using MyPlaces.Api.Features.Auth;

namespace MyPlaces.Api.Common.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1");

        // Auth
        v1.MapPost("/auth/register", Register.Handle);
        v1.MapPost("/auth/login", Login.Handle);
        v1.MapPost("/auth/google", GoogleLogin.Handle);
        v1.MapPost("/auth/refresh-token", RefreshToken.Handle);

        return app;
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build src/MyPlaces.Api/MyPlaces.Api.csproj
```

Expected: Build succeeded (có thể có warning về Features/Auth chưa tồn tại — sẽ fix ở Task 5).

- [ ] **Step 5: Commit**

```bash
git add src/MyPlaces.Api/Common/CurrentUser.cs src/MyPlaces.Api/Common/Extensions/
git commit -m "feat: add ICurrentUser and auth/endpoint extensions"
```

---

## Task 5: Api — Auth Features (Register, Login, GoogleLogin, RefreshToken)

**Files:**
- Create: `src/MyPlaces.Api/Features/Auth/Register.cs`
- Create: `src/MyPlaces.Api/Features/Auth/Login.cs`
- Create: `src/MyPlaces.Api/Features/Auth/GoogleLogin.cs`
- Create: `src/MyPlaces.Api/Features/Auth/RefreshToken.cs`
- Create: `src/MyPlaces.Api/Common/JwtTokenService.cs`

- [ ] **Step 1: Tạo JwtTokenService**

Tạo `src/MyPlaces.Api/Common/JwtTokenService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Common;

public class JwtTokenService(IConfiguration config)
{
    public string GenerateToken(AppUser user)
    {
        var secret = config["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("username", user.Username),
            new Claim("displayName", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

- [ ] **Step 2: Tạo Register feature**

Tạo `src/MyPlaces.Api/Features/Auth/Register.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class Register
{
    public static async Task<IResult> Handle(
        RegisterRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        if (await userManager.FindByEmailAsync(req.Email) is not null)
            return Results.BadRequest(Result<AuthResponse>.Failure("Email already in use"));

        if (await userManager.FindByNameAsync(req.Username) is not null)
            return Results.BadRequest(Result<AuthResponse>.Failure("Username already taken"));

        var user = new AppUser
        {
            Email = req.Email,
            UserName = req.Email,
            Username = req.Username,
            DisplayName = req.DisplayName
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(Result<AuthResponse>.Failure(errors));
        }

        var token = tokenService.GenerateToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        // Store refresh token (plain string for MVP — hash in production)
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", refreshToken);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
```

- [ ] **Step 3: Tạo Login feature**

Tạo `src/MyPlaces.Api/Features/Auth/Login.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class Login
{
    public static async Task<IResult> Handle(
        LoginRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", refreshToken);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
```

- [ ] **Step 4: Tạo GoogleLogin feature**

Tạo `src/MyPlaces.Api/Features/Auth/GoogleLogin.cs`:

```csharp
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public static class GoogleLogin
{
    public static async Task<IResult> Handle(
        GoogleLoginRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService,
        IConfiguration config)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [config["Google:ClientId"]]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
        }
        catch
        {
            return Results.BadRequest(Result<AuthResponse>.Failure("Invalid Google token"));
        }

        var user = await userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            // Auto-register with Google
            var username = payload.Email.Split('@')[0].ToLower().Replace(".", "_");
            user = new AppUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                Username = username,
                DisplayName = payload.Name ?? username,
                AvatarUrl = payload.Picture,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Results.BadRequest(Result<AuthResponse>.Failure(errors));
            }
        }

        var token = tokenService.GenerateToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", refreshToken);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
```

- [ ] **Step 5: Tạo RefreshToken feature**

Tạo `src/MyPlaces.Api/Features/Auth/RefreshToken.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Features.Auth;

public record RefreshTokenRequest(string UserId, string RefreshToken);

public static class RefreshToken
{
    public static async Task<IResult> Handle(
        RefreshTokenRequest req,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        var user = await userManager.FindByIdAsync(req.UserId);
        if (user is null)
            return Results.Unauthorized();

        var stored = await userManager.GetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken");
        if (stored != req.RefreshToken)
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        var newRefresh = tokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, "MyPlaces", "RefreshToken", newRefresh);

        return Results.Ok(Result<AuthResponse>.Success(new AuthResponse(
            Token: token,
            RefreshToken: newRefresh,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            UserId: user.Id.ToString(),
            Email: user.Email!,
            Username: user.Username,
            DisplayName: user.DisplayName
        )));
    }
}
```

- [ ] **Step 6: Build**

```bash
dotnet build src/MyPlaces.Api/MyPlaces.Api.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add src/MyPlaces.Api/Features/Auth/ src/MyPlaces.Api/Common/JwtTokenService.cs
git commit -m "feat: add auth features (register, login, google, refresh-token)"
```

---

## Task 6: Api — Program.cs và appsettings

**Files:**
- Modify: `src/MyPlaces.Api/Program.cs`
- Create: `src/MyPlaces.Api/appsettings.json`
- Create: `src/MyPlaces.Api/appsettings.Development.json`

- [ ] **Step 1: Cập nhật Program.cs**

Thay toàn bộ nội dung `src/MyPlaces.Api/Program.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Auth
builder.Services.AddJwtAuth(builder.Configuration);

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<JwtTokenService>();

// Validation (.NET 10 built-in)
builder.Services.AddValidation();

// CORS (Blazor WASM)
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAllEndpoints();

// Auto-migrate on startup (dev only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program { } // For integration tests
```

- [ ] **Step 2: Cập nhật appsettings.json**

Thay nội dung `src/MyPlaces.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_NEON_HOST;Database=myplaces;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require"
  },
  "Jwt": {
    "Secret": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS",
    "Issuer": "MyPlaces",
    "Audience": "MyPlaces"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID"
  },
  "AllowedOrigins": "https://your-static-web-app.azurestaticapps.net",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Tạo appsettings.Development.json**

Tạo `src/MyPlaces.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=myplaces_dev;Username=myplaces;Password=myplaces_dev_password"
  },
  "Jwt": {
    "Secret": "dev_secret_min_32_chars_change_in_prod_x",
    "Issuer": "MyPlaces",
    "Audience": "MyPlaces"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID"
  },
  "AllowedOrigins": "http://localhost:5001"
}
```

- [ ] **Step 4: Thêm appsettings secrets vào .gitignore**

Mở `.gitignore` và thêm:

```
# Secrets
src/MyPlaces.Api/appsettings.Development.json
*.user
```

- [ ] **Step 5: Build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/MyPlaces.Api/Program.cs src/MyPlaces.Api/appsettings.json .gitignore
git commit -m "feat: configure Program.cs with Identity, JWT, CORS, and OpenAPI"
```

---

## Task 7: EF Core Migration

**Files:**
- Create: `src/MyPlaces.Api/Migrations/` (auto-generated)

- [ ] **Step 1: Install EF Core tools nếu chưa có**

```bash
dotnet tool install --global dotnet-ef
```

Expected: Tool installed hoặc "already installed".

- [ ] **Step 2: Đảm bảo Docker PostgreSQL đang chạy**

```bash
docker compose ps
```

Expected: `myplaces-postgres` status `running`. Nếu chưa chạy: `docker compose up -d`.

- [ ] **Step 3: Tạo migration đầu tiên**

```bash
cd src/MyPlaces.Api
dotnet ef migrations add InitialCreate --output-dir Migrations
```

Expected: Migration files được tạo trong `Migrations/`.

- [ ] **Step 4: Apply migration lên Neon**

```bash
dotnet ef database update
```

Expected: "Done." — tables được tạo trên Neon PostgreSQL.

- [ ] **Step 5: Verify tables tồn tại**

Vào pgAdmin tại `http://localhost:5050` → connect đến server `myplaces-postgres` (host: `postgres`, user: `myplaces`, password: `myplaces_dev_password`) → Query Tool:

```sql
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

Expected: Thấy `asp_net_users`, `places`, `trips`, `trip_places`, `place_photos`, `follows`.

- [ ] **Step 6: Commit**

```bash
cd ../..
git add src/MyPlaces.Api/Migrations/
git commit -m "feat: add initial EF Core migration"
```

---

## Task 8: Tests — Auth Integration Tests

**Files:**
- Create: `tests/MyPlaces.Api.Tests/Features/Auth/RegisterTests.cs`
- Create: `tests/MyPlaces.Api.Tests/Features/Auth/LoginTests.cs`
- Create: `tests/MyPlaces.Api.Tests/TestWebAppFactory.cs`

- [ ] **Step 1: Tạo TestWebAppFactory**

Tạo `tests/MyPlaces.Api.Tests/TestWebAppFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPlaces.Api.Common;

namespace MyPlaces.Api.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
        });

        builder.UseEnvironment("Testing");
    }
}
```

- [ ] **Step 2: Tạo Register tests**

Tạo `tests/MyPlaces.Api.Tests/Features/Auth/RegisterTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Tests.Features.Auth;

public class RegisterTests(TestWebAppFactory factory)
    : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1",
            DisplayName: "Test User"
        );

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeEmpty();
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest(
            Email: "dup@example.com",
            Username: "dupuser",
            Password: "Password1",
            DisplayName: "Dup User"
        );

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

- [ ] **Step 3: Tạo Login tests**

Tạo `tests/MyPlaces.Api.Tests/Features/Auth/LoginTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Tests.Features.Auth;

public class LoginTests(TestWebAppFactory factory)
    : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Register first
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            Email: "login@example.com",
            Username: "loginuser",
            Password: "Password1",
            DisplayName: "Login User"
        ));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(Email: "login@example.com", Password: "Password1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            Email: "wrong@example.com",
            Username: "wronguser",
            Password: "Password1",
            DisplayName: "Wrong User"
        ));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(Email: "wrong@example.com", Password: "WrongPassword1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Step 4: Chạy tests**

```bash
dotnet test tests/MyPlaces.Api.Tests/
```

Expected: All tests pass (4 tests).

- [ ] **Step 5: Commit**

```bash
git add tests/
git commit -m "test: add auth integration tests (register, login)"
```

---

## Task 9: Verify API chạy locally

- [ ] **Step 1: Chạy API**

```bash
cd src/MyPlaces.Api
dotnet run
```

Expected: API chạy tại `http://localhost:5000` (hoặc port trong launchSettings.json).

- [ ] **Step 2: Test OpenAPI**

Mở browser: `http://localhost:5000/openapi/v1.json`

Expected: JSON schema với các endpoints `/api/v1/auth/register`, `/api/v1/auth/login`, etc.

- [ ] **Step 3: Test Register endpoint**

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"phuc@example.com","username":"phuc","password":"Password1","displayName":"Phuc Thai"}'
```

Expected: `{"isSuccess":true,"value":{"token":"eyJ...","refreshToken":"...","expiresAt":"..."}}`

- [ ] **Step 4: Commit final**

```bash
git add -A
git commit -m "chore: verify API runs correctly locally"
```

---

## Self-Review

**Spec coverage check:**
- ✅ Docker local dev (PostgreSQL + pgAdmin) — Task 0
- ✅ .NET 10 Minimal API — Task 1, 6
- ✅ EF Core 10 + Neon PostgreSQL — Task 3, 7
- ✅ ASP.NET Core Identity + JWT — Task 4, 5
- ✅ Google OAuth — Task 5 (GoogleLogin.cs)
- ✅ Result<T> pattern — Task 2
- ✅ Refresh token — Task 5 (RefreshToken.cs)
- ✅ Database indexes — Task 3 (AppDbContext)
- ✅ Vertical Slice structure — Task 5
- ✅ ICurrentUser — Task 4
- ✅ Integration tests — Task 8

**Không cover trong Plan 1 (sẽ ở plan sau):**
- Places, Trips, Photos, Social, Dashboard — Plan 2-6
- Blazor WASM client — Plan 2+
- Forgot password — Plan 2 (Auth extension)

**Placeholder scan:** Không có TBD hay TODO trong implementation steps. Connection string và secrets có placeholder rõ ràng với hướng dẫn điền.

**Type consistency:** `AuthResponse`, `Result<T>`, `AppUser`, `JwtTokenService` dùng nhất quán trong tất cả tasks.
