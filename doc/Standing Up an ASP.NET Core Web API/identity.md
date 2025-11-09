---
order: 2
icon: gear
---
# Identity in your Web API

## START FROM THE END OF THE WORKSHOP FINAL PROJECT
[Documenting your API with OpenAPI](../../standing-up-an-asp.net-core-web-api/documenting-with-openapi/)

## Module overview and outcomes
- Objective: Add ASP.NET Core Identity, JWT-based authentication, and authorization to the existing Web API, and prepare Swagger/OpenAPI for secured testing. No UI; pure API.
- What you’ll deliver
    - Identity store and user/role management via EF Core
    - Token issuance endpoints (register/login) with JWT
    - Policies and role-based authorization protecting selected endpoints
    - Swagger configured with versioning and “Authorize” (Bearer) support
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

4) Developer experience and Swagger
- Add Swagger with versioned docs (using already-configured `IApiVersionDescriptionProvider`).
- Add a Bearer security scheme so tokens can be pasted into Swagger UI.

5) Deployment considerations
- Secrets via user-secrets in Development, environment variables/Key Vault in Non-Dev.
- EF migrations to create Identity schema.

---

## Detailed, step-by-step implementation guide
Note: File paths are relative to `Chinook.API` unless stated. Replace provider-specific lines for your database.

### Step 1 Add packages
- Core packages
    - `dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore`
    - `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`
    - `dotnet add package Swashbuckle.AspNetCore`
- EF Core provider (pick one)
    - SQL Server: `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
    - SQLite: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
    - PostgreSQL: `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`

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
    "IdentityConnection": "<SET IN user-secrets OR ENV>"
  },
  "Jwt": {
    "Issuer": "Chinook.Api",
    "Audience": "Chinook.Api.Clients",
    "SigningKey": "<DEVELOPMENT-ONLY-KEY-USE-USER-SECRETS>"
  }
}
```
- Set Development values via user-secrets (example for SQL Server):
```bash
# Inside Chinook.API directory
dotnet user-secrets set "ConnectionStrings:IdentityConnection" "Server=localhost;Database=ChinookIdentity;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SigningKey" "dev-super-secret-signing-key-change-me"
```

### Step 4 Register DbContext, Identity, Authentication, Authorization in `Program.cs`
- Using statements to add:
```csharp
using System.Text;
using Chinook.API.Data;
using Chinook.API.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
```
- Service registrations (in the builder.Services section):
```csharp
// DbContext for Identity (choose provider)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Identity Core
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
    .AddDefaultTokenProviders();

// Authentication with JWT Bearer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:SigningKey"]))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanWrite", policy => policy.RequireRole("Admin", "Manager"));
});
```
- Middleware order (in the pipeline before `app.MapControllers();`):
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### Step 5 Implement a token service
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

### Step 6 Add authentication endpoints
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

### Step 7 Seed default roles and an admin user
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

### Step 8 Protect existing API endpoints
- Strategy for `AlbumController` and others
    - Add `[Authorize]` at the controller level to require authentication for all actions, then place `[AllowAnonymous]` on the `GET` actions if you want them public.
    - For write operations, add `[Authorize(Policy = "CanWrite")]`.
- Example attribute usage:
```csharp
[Authorize] // at controller level
public class AlbumController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public Task<...> Get() { ... }

    [Authorize(Policy = "CanWrite")] 
    [HttpPost]
    public Task<...> Post(...) { ... }
}
```

### Step 9 Add Swagger/OpenAPI with JWT support and API versioning
- Services (in `Program.cs`):
```csharp
using Microsoft.OpenApi.Models;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Versioned docs: you can dynamically add docs using IApiVersionDescriptionProvider later
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chinook API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});
```
- Middleware and UI (after building `app`):
```csharp
app.UseSwagger();
app.UseSwaggerUI();
```
- Enhancement for API versioning: inject `IApiVersionDescriptionProvider` and enumerate to call `SwaggerEndpoint` per version group in `UseSwaggerUI`.

### Step 10 Create and apply EF Core migrations
- Add migration and update database:
```bash
# From Chinook.API folder
 dotnet ef migrations add AddIdentitySchema --context ApplicationDbContext
 dotnet ef database update --context ApplicationDbContext
```
- Verify Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) are created.

### Step 11 Manual test plan (quick pass)
- Register a user via `POST /api/v1/auth/register` with JSON body `{ "userName": "alice", "email": "alice@local", "password": "Alice123!" }`.
- Login via `POST /api/v1/auth/login`, copy `access_token`.
- In Swagger, click `Authorize` and paste `Bearer {token}`.
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
- Swagger/OpenAPI
    - Swagger UI is available in Development.
    - “Authorize” button accepts a Bearer token; authenticated calls succeed via Swagger.
    - Versioned endpoints appear under correct version groups.
- Configuration and security hygiene
    - JWT signing key and connection strings are stored in user-secrets or environment variables, not committed.
    - Password policy documented; defaults appropriate for workshop.
- Documentation and testing
    - README (or module guide) includes setup steps, testing instructions, and example requests.
    - Manual test steps executed and passed (register → login → call protected & public endpoints).