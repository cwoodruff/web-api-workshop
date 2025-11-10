---
order: 2
icon: gear
---
# Identity in your Web API

## START FROM THE END OF THE WORKSHOP FINAL PROJECT
[Documenting your API with OpenAPI](../../standing-up-an-asp.net-core-web-api/documenting-with-openapi/)

## Module overview and outcomes
- Objective: Add ASP.NET Core Identity, JWT-based authentication, and authorization to the existing Web API, and prepare for Swagger/OpenAPI in next part for secured testing. No UI; pure API.
- What you’ll deliver
    - Identity store and user/role management via EF Core
    - Token issuance endpoints (register/login) with JWT
    - Policies and role-based authorization protecting selected endpoints
- Prerequisites
    - .NET SDK matching `net10.0` target
    - Chosen DB provider installed locally (SQL Server / SQLite / PostgreSQL)
    - EF Core tools: `dotnet tool install --global dotnet-ef`
    - Development secret storage configured (`dotnet user-secrets init` in API project directory)

---

## High-level plan (architecture and key decisions)
1) Identity storage and models
- Use EF Core with `ApplicationDbContext` dedicated to Identity tables.
- Start with `ApplicationUser : IdentityUser` (extend later if needed) and optional `ApplicationRole : IdentityRole`.

2) Authentication and token strategy
- Primary authentication: JWT Bearer access tokens signed with symmetric key in Development (user-secrets).
- Token contents: standard claims (`sub`, `nameid`, `unique_name`, `role`), plus optional custom claims later.

3) Authorization strategy
- Role-based policies: `Admin`, `Manager`, `User` with a `CanWrite` policy for write operations.
- Default stance: Secure write endpoints; keep read endpoints public or authenticated based on workshop goals.

4) Deployment considerations
- Secrets via user-secrets in Development, environment variables/Key Vault in Non-Dev.
- EF migrations to create Identity schema.

---

## Detailed, step-by-step implementation guide
Note: File paths are relative to `Chinook.API` unless stated. Replace provider-specific lines for your database.

### Step 1 Add packages
- Core packages
    - `dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore`
    - `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`

### Step 2 Create Identity models and DbContext
- Add `Identity/ApplicationUser.cs`:
```csharp
using Microsoft.AspNetCore.Identity;

namespace Chinook.API.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Add profile fields later if needed, e.g. FirstName, LastName
    }
}
```
- (Optional) `Identity/ApplicationRole.cs`:
```csharp
using Microsoft.AspNetCore.Identity;

namespace Chinook.API.Identity
{
    public class ApplicationRole : IdentityRole { }
}
```
- Add `Data/ApplicationDbContext.cs`:
```csharp
using Chinook.API.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
```

### Step 3 Add configuration settings and secrets
- In `appsettings.json`, add (placeholders):
```json
{
  "ConnectionStrings": {
    // Existing SQL Server connections (already in your project)
    "ChinookDbWindows": "Server=.;Database=Chinook;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=true;Trusted_Connection=true;Application Name=ChinookWebAPI",
    "ChinookDbDocker": "Server=localhost,2022;Database=Chinook;User=sa;Password=8Riwudeg!!;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=true;Application Name=ChinookWebAPI",

    // Option B (separate Identity DB) – SQL Server variant
    "IdentitySqlServer": "Server=.;Database=ChinookIdentity;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=true;Trusted_Connection=true;Application Name=ChinookWebAPI",

    // Option B (separate Identity DB) – SQLite variant
    // Creates/uses a local file ChinookIdentity.db in the API folder
    "IdentitySqlite": "Data Source=ChinookIdentity.db;Cache=Shared"
  },
  "Identity": {
    // Choose which provider to use for the Identity context when on Option B
    // Values: "SqlServer" or "Sqlite"
    "Provider": "SqlServer"
  },
  "Jwt": {
    "Issuer": "Chinook.Api",
    "Audience": "Chinook.Api.Clients",
    "SigningKey": "<SET_WITH_USER_SECRETS>"
  }
}
```
Development secrets (recommended):
- Set `Jwt:SigningKey` using user‑secrets.
- If you override any connection strings locally, set them via user‑secrets as well.

```bash
 cd Chinook.API
 dotnet user-secrets init
 dotnet user-secrets set "Jwt:SigningKey" "dev-super-secret-signing-key-change-me"
```

### Step 4 Register DbContext, Identity, Authentication, Authorization in `Program.cs`
You have two solid ways to wire the Identity `ApplicationDbContext`.

#### Option A — Reuse the existing Chinook SQL Server connection (simple)
This is already close to what you have. Keep or paste this block where services are configured (before Identity is added):

```csharp
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Chinook.API.Data;

// Decide at runtime which Chinook SQL connection to use
var connection = string.Empty;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    connection = builder.Configuration.GetConnectionString("ChinookDbWindows");
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    connection = builder.Configuration.GetConnectionString("ChinookDbDocker");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
```

Pros: one database to manage; fewer moving parts for a workshop.
Cons: Identity tables mix with application tables (OK for workshops, less ideal for prod).

#### Option B — Separate Identity DB with a switch for SQL Server or SQLite
Use this when you want to switch Identity storage independently (SQL Server vs SQLite) without touching Chinook’s main DB.

```csharp
using Microsoft.EntityFrameworkCore;
using Chinook.API.Data;

var idProvider = builder.Configuration["Identity:Provider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (idProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = builder.Configuration.GetConnectionString("IdentitySqlite")
                         ?? "Data Source=ChinookIdentity.db;Cache=Shared";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        // Default to SQL Server for Identity DB
        var sqlConn = builder.Configuration.GetConnectionString("IdentitySqlServer")
                      ?? builder.Configuration.GetConnectionString(
                           RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                               ? "ChinookDbWindows"
                               : "ChinookDbDocker");
        options.UseSqlServer(sqlConn);
    }
});
```

Notes:
- For SQLite, ensure the `Microsoft.EntityFrameworkCore.Sqlite` package is referenced in `Chinook.API.csproj` if you choose this provider:
  ```xml
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-rc.2.25502.107" />
  ```
- For SQL Server, you already have `Microsoft.EntityFrameworkCore.SqlServer` referenced.

Identity + JWT + Authorization registrations remain as you have them (no change needed):

```csharp
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cfg = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = cfg["Jwt:Issuer"],
            ValidAudience = cfg["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:SigningKey"] ?? "")),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanWrite", policy => policy.RequireRole("Admin", "Manager"));
});
```

Pipeline (unchanged):
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### Step 5 EF Core migrations per provider
Run from the `Chinook.API` directory.

- SQL Server (Option A or Option B with SqlServer):
```bash
 dotnet ef migrations add AddIdentitySchema --context ApplicationDbContext
 dotnet ef database update --context ApplicationDbContext
```

- SQLite (Option B):
```bash
# Add the Sqlite provider package first if not present
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.0-rc.2.25502.107

 dotnet ef migrations add AddIdentitySchema_Sqlite --context ApplicationDbContext
 dotnet ef database update --context ApplicationDbContext
```

- Verify Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) are created.

### Step 6 Implement a token service
- Add `Services/ITokenService.cs`:
```csharp
using System.Security.Claims;

namespace Chinook.API.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(IEnumerable<Claim> claims, DateTimeOffset? expires = null);
    }
}
```
- Add `Services/TokenService.cs`:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Chinook.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public string CreateAccessToken(IEnumerable<Claim> claims, DateTimeOffset? expires = null)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: (expires ?? DateTimeOffset.UtcNow.AddHours(1)).UtcDateTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```
- Register in DI (in `Program.cs`): `builder.Services.AddSingleton<ITokenService, TokenService>();`

### Step 7 Add authentication endpoints
- Add `Controllers/AuthController.cs`:
```csharp
using System.Security.Claims;
using Chinook.API.Identity;
using Chinook.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public record RegisterDto(string UserName, string Email, string Password);
        public record LoginDto(string UserName, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return CreatedAtAction(nameof(Register), new { dto.UserName }, new { dto.UserName, dto.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) return Unauthorized();

            var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordOk) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = _tokenService.CreateAccessToken(claims);
            return Ok(new { access_token = token, token_type = "Bearer" });
        }
    }
}
```
- Note: `SignInManager` is optional for API; `UserManager` + password check is sufficient.

### Step 8 Seed default roles and an admin user
- Add `Infrastructure/IdentityDataSeeder.cs`:
```csharp
using Chinook.API.Identity;
using Microsoft.AspNetCore.Identity;

namespace Chinook.API.Infrastructure
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = ["Admin", "Manager", "User"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminUserName = "admin";
            var adminEmail = "admin@chinook.local";
            var admin = await userManager.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminUserName, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
```
- Invoke seeding at startup (after `app` is built):
```csharp
await Chinook.API.Infrastructure.IdentityDataSeeder.SeedAsync(app.Services);
```

### Step 9 Protect existing API endpoints
- Strategy for `AlbumController` and others
    - Add `[Authorize]` at the controller level to require authentication for all actions, then place `[AllowAnonymous]` on the `GET` actions if you want them public.
    - For write operations, add `[Authorize(Policy = "CanWrite")]`.

The `AlbumController` in your repo already matches the recommended pattern. Here’s the authoritative template to apply elsewhere:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize] // default: all actions require an authenticated user
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous] // explicitly allow public, unauthenticated GETs
    public async Task<IActionResult> GetAll() { /* ... */ }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) { /* ... */ }

    [HttpPost]
    [Authorize(Policy = "CanWrite")] // only Admin/Manager (per policy) can write
    public async Task<IActionResult> Create([FromBody] object dto) { /* ... */ }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanWrite")] // protect updates
    public async Task<IActionResult> Update(int id, [FromBody] object dto) { /* ... */ }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanWrite")] // protect deletes
    public async Task<IActionResult> Delete(int id) { /* ... */ }
}
```

Use `[AllowAnonymous]` only where you truly want open access. Otherwise, everything under `[Authorize]` will enforce JWT auth, returning 401 for missing/invalid tokens and 403 for insufficient roles.

### Step 10 Manual test plan (quick pass)
- Register a user via `POST /api/v1/auth/register` with JSON body `{ "userName": "alice", "email": "alice@local", "password": "Alice123!" }`.
- Login via `POST /api/v1/auth/login`, copy `access_token`.
- Call a protected write endpoint (e.g., `POST /api/v1/albums`) without token and verify 401.
- Call a protected write endpoint (e.g., `POST /api/v1/albums`) and verify 403 for a normal user.
- Login as admin (seeded), paste token, and verify write succeeds (200/201).
- Call a public `GET` without token and confirm it works if `[AllowAnonymous]` is applied.
---

## Acceptance checklist (Definition of Done)
- Identity and database
    - `ApplicationDbContext` exists and migrations are applied; Identity tables present.
    - Seeding creates roles `Admin`, `Manager`, `User` and a default admin user.
- Authentication
    - `AddAuthentication` with JWT Bearer is configured.
    - `UseAuthentication` is placed before `UseAuthorization` in the pipeline.
    - Login endpoint returns a valid JWT; token includes user id, name, and assigned roles.
- Authorization
    - Authorization policies exist (at least `CanWrite`).
    - Write endpoints return 401 without token and 403 for authenticated users without sufficient role.
    - Admin token can access write endpoints successfully.
- Configuration and security hygiene
    - JWT signing key and connection strings are stored in user-secrets or environment variables, not committed.
    - Password policy documented; defaults appropriate for workshop.