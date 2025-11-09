---
order: 1
icon: gear
---
# Versioning your Web API

## Overview
This plan adds API versioning using URL segments (e.g., `/api/v1/albums`) to cleanly separate versions, work seamlessly with HTTP response caching, and prepare for multi-version Swagger. You’ll:
- Install API versioning packages
- Register API versioning in `Program.cs`
- Annotate controllers and adjust routes
- Prepare for deprecation/sunset
- Validate caching behavior
- Add test coverage

Why URL segments? They naturally partition caches (path differs per version), are easy to reason about, and Swagger grouping is straightforward. Your domain cache (`VersionedMemoryCache`) remains unchanged, because API versioning is a surface concern.

Where you will code:
- `Chinook.API/Program.cs` (service registration)
- `Chinook.API/Controllers/*Controller.cs` (attributes/routes)
- Optional: custom middleware/filter for deprecation headers
- Tests: new test project or existing test suite

---

## Step 1 — Add NuGet dependencies
Why: Brings in the MVC API versioning services and the API explorer needed later for Swagger to emit one document per version.

- API versioning core:
    - `Microsoft.AspNetCore.Mvc.Versioning`
- API explorer integration (for Swagger grouping later):
    - `Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer`

Commands:
```
dotnet add Chinook.API package Microsoft.AspNetCore.Mvc.Versioning
dotnet add Chinook.API package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
```

---

## Step 2 — Register API versioning in Program.cs
Why: Configures how versions are read (URL segment), defaulting behavior, and exposes helpful headers.

Edit `Chinook.API/Program.cs` and add the following service registrations right after controllers are added (or before building the app):

```csharp
using Microsoft.AspNetCore.Mvc; // for ApiVersion
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer; // later for Swagger

var builder = WebApplication.CreateBuilder(args);

// ... existing service registrations
builder.Services.AddControllers();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false; // force explicit versioning
    options.ReportApiVersions = true; // adds api-supported-versions, api-deprecated-versions headers
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // version in the URL path
});

// Versioned API Explorer (used by Swagger later)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // e.g., v1, v1.1
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// ... existing middleware (UseCors, UseResponseCaching, etc.)
```

Notes:
- Keep `app.UseResponseCaching()` as-is. URL segment versioning inherently splits cached entries by path.
- If you prefer a soft rollout, temporarily set `AssumeDefaultVersionWhenUnspecified = true` and remove it after client migration.

---

## Step 3 — Annotate controllers and update route templates
Why: Declares supported versions and makes the version part of routes like `/api/v1/album`.

For each API controller under `Chinook.API/Controllers`:
1) Add an API version attribute at the class level
2) Change the route to include a version URL segment

Example for `AlbumController`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[ApiVersion("1.0")] // declare this controller belongs to v1
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
public class AlbumController : ControllerBase
{
    // ... existing code
}
```

Repeat for each controller (Artist, Customer, Employee, Genre, Invoice, InvoiceLine, MediaType, Playlist, Track, etc.).

Version-neutral endpoints (e.g., health/home):
```csharp
[ApiVersionNeutral]
[Route("/health")] // or whatever your endpoint is
public class HealthController : ControllerBase { /* ... */ }
```

Notes:
- With URL segments, you do NOT need to add `Vary` directives for response caching—the path differs by version.
- When you introduce v2, you can either:
    - Create separate controller classes in a `V2` folder/namespace with `[ApiVersion("2.0")]`, same route template; or
    - Keep a single controller and map specific actions:
      ```csharp
      [ApiVersion("1.0")]
      [ApiVersion("2.0")]
      [Route("api/v{version:apiVersion}/[controller]")]
      public class AlbumController : ControllerBase
      {
          [HttpGet]
          [MapToApiVersion("1.0")] // v1 behavior
          public IActionResult GetV1() => Ok(/* ... */);
  
          [HttpGet]
          [MapToApiVersion("2.0")] // v2 behavior
          public IActionResult GetV2() => Ok(/* ... */);
      }
      ```

---

## Step 4 — Prepare for deprecation and sunset (optional but recommended)
Why: Communicates lifecycle changes to clients proactively.

- Mark deprecated versions at the controller level:
```csharp
[ApiVersion("1.0", Deprecated = true)]
```
- Add a custom middleware or action filter to inject `Sunset` and documentation links for deprecated versions:
```csharp
public class SunsetHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DateTimeOffset _sunsetDate;

    public SunsetHeaderMiddleware(RequestDelegate next, DateTimeOffset sunsetDate)
    {
        _next = next;
        _sunsetDate = sunsetDate;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);
        // conditionally add header for deprecated versions only, if desired
        context.Response.Headers.TryAdd("Sunset", _sunsetDate.ToString("R"));
        context.Response.Headers.TryAdd("Link", "</docs/migration>; rel=\"deprecation\"");
    }
}

// In Program.cs
app.UseMiddleware<SunsetHeaderMiddleware>(new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero));
```

---

## Step 5 — Validate and align with response caching
Why: Ensure caches don’t mix content across versions.

- Keep `app.UseResponseCaching()`.
- If you add `[ResponseCache]` attributes, you can keep them as-is with URL segment versioning.
- If at any point you switch to header or query-string versioning, add appropriate varies:
    - Header-based (e.g., `x-api-version`):
      ```csharp
      [ResponseCache(VaryByHeader = "x-api-version")]
      ```
    - Query-based (e.g., `api-version`):
      ```csharp
      [ResponseCache(VaryByQueryKeys = new[] { "api-version" })]
      ```

Your domain-level `VersionedMemoryCache` does not require changes—API versioning doesn’t affect those keys.

---

## Step 6 — Rollout plan and compatibility window
Why: Avoid breaking existing clients overnight.

- Option A (strict): Require `/api/v1/...` immediately (`AssumeDefaultVersionWhenUnspecified = false`).
- Option B (graceful): Temporarily allow unversioned routes:
    - Set `AssumeDefaultVersionWhenUnspecified = true` so old `/api/[controller]` resolves to v1.
    - Communicate migration deadline and later revert to `false`.
- Option C (dual endpoints): Expose both old and new routes by keeping a compatibility controller or duplicate `[Route]` attributes for a short window, then remove the legacy route.

---

## Step 7 — Testing the versioning (unit/integration)
Why: Proves routing, version selection, headers, and caching behavior are correct.

- Test harness: `Microsoft.AspNetCore.Mvc.Testing` with `WebApplicationFactory<Program>` to run the API in-memory.

Example tests (xUnit-style):
```csharp
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class VersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VersioningTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_V1_Album_Should_Return_200()
    {
        var res = await _client.GetAsync("/api/v1/album");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.True(res.Headers.Contains("api-supported-versions"));
    }

    [Fact]
    public async Task Get_V2_Album_Should_Return_404_When_Not_Implemented()
    {
        var res = await _client.GetAsync("/api/v2/album");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
```

Caching differentiation test (conceptual):
```csharp
[Fact]
public async Task ResponseCache_Should_Not_Mix_Versions()
{
    // First request to v1
    var res1 = await _client.GetAsync("/api/v1/album");
    res1.EnsureSuccessStatusCode();

    // Then to v2 (assuming different behavior or not implemented)
    var res2 = await _client.GetAsync("/api/v2/album");

    // Expect different status/content; at minimum, not the same response object
    Assert.NotEqual(res1.StatusCode, res2.StatusCode);
}
```

Domain cache independence (with repository mocks) — pseudocode outline:
```csharp
// Arrange a test server with mocked repository and in-memory IMemoryCache
// Call /api/v1/album to populate domain cache via supervisor
// Call /api/v2/album (same underlying domain call) and assert repository call count has not increased
// demonstrating domain cache is shared across API versions
```

Swagger API Explorer (when added):
```csharp
// Resolve IApiVersionDescriptionProvider, assert it contains v1, and v1 is marked deprecated when you set Deprecated = true
```

---

## Step 8 — Prepare for Swagger (next iteration)
Why: You’ll want separate Swagger docs per version.

When you add Swashbuckle:
```csharp
using Microsoft.AspNetCore.Mvc.ApiExplorer;

builder.Services.AddSwaggerGen(options =>
{
    using var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = $"Chinook API {description.ApiVersion}",
            Version = description.GroupName,
            Description = description.IsDeprecated ? "This API version has been deprecated." : null
        });
    }
});

// app.UseSwagger(); app.UseSwaggerUI(c => { ... add endpoints per description ... });
```

---

## Acceptance checklist
- All controllers routed under `/api/v1/...` and annotated with `[ApiVersion("1.0")]`.
- `AddApiVersioning` and `AddVersionedApiExplorer` registered.
- Response caching remains valid (no mixing across versions).
- Optional deprecation/sunset headers in place when needed.
- Tests passing for routing, headers, cache behavior, and version explorer.
- Ready to add Swagger with one doc per version.

If you’d like, I can turn this plan into a file-by-file change checklist for your repo and prepare a starting PR once you confirm the approach.