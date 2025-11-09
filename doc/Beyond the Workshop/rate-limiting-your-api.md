---
order: 1
icon: rocket
---
# Rate Limiting your API

## Implementing 20 requests/min Rate Limiting in ASP.NET Core 10 Web API

## Overview and Objectives

Goal: Add a simple, demo-friendly rate limit of 20 requests per minute per client to all controller endpoints.

Chosen approach:
- Limiter type: fixed-window (1 minute)
- Partitioning: per client IP address
- Policy name: `fixed-per-minute`
- Middleware: enable `UseRateLimiter()` in the pipeline
- Application: apply policy to all controllers via endpoint convention
- Rejection behavior: return `429 Too Many Requests` with `Retry-After: 60` and a brief message

Result: After 20 successful requests from the same IP within a minute, further requests receive 429 until the window resets.

Key namespaces:
- `Microsoft.AspNetCore.RateLimiting`
- `System.Threading.RateLimiting`

Key files:
- `Chinook.API/Configurations/ServicesConfiguration.cs`
- `Chinook.API/Program.cs`

---

## Prerequisites and Assumptions

- Project targets .NET 10 (per environment info) and uses ASP.NET Core 10.
- You have the solution open in Rider/VS, or a terminal with dotnet CLI.
- No gateway/ingress rewriting client IPs without forwarded headers configuration (for demo). If behind a proxy/LB, see Production Notes below.

---

## Implementation

The following steps describe precisely what’s already implemented in the solution.

#### Step 1 — Add required namespaces and service extension

File: `Chinook.API/Configurations/ServicesConfiguration.cs`

- Ensure the following namespaces are present at the top:
    - `using Microsoft.AspNetCore.Http;`
    - `using Microsoft.AspNetCore.RateLimiting;`
    - `using System.Threading.RateLimiting;`

- Add an extension method to register the rate limiter in DI (this is already present):

```csharp
public static void AddRateLimitingServices(this IServiceCollection services)
{
    // Configure a named per-IP fixed window limiter policy (20 requests per minute)
    services.AddRateLimiter(options =>
    {
        // Named policy: per-IP fixed window limiter (20 requests per minute)
        options.AddPolicy<string>("fixed-per-minute", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            // Simple Retry-After header for demo purposes
            context.HttpContext.Response.Headers.RetryAfter = "60";
            await context.HttpContext.Response.WriteAsync("Too many requests. Please retry later.", token);
        };
    });
}
```

What this does:
- Creates a named policy `fixed-per-minute`.
- Uses per-IP partitioning so each client IP gets its own 20-requests/min limit.
- Disables queuing (`QueueLimit = 0`) to make demo behavior crisp.
- Sets a clear `OnRejected` handler to return `429` with `Retry-After: 60` seconds.


#### Step 2 — Register the extension in Program.cs

File: `Chinook.API/Program.cs`

- Ensure this namespace is present for endpoint conventions:
```csharp
using Microsoft.AspNetCore.RateLimiting;
```

- After the existing service registrations and `AddControllers()`, register your rate-limiting services:
```csharp
builder.Services.AddRateLimitingServices();
```


#### Step 3 — Add middleware in the correct order

- Insert the rate limiter into the middleware pipeline before `UseAuthorization()` and after `UseCors()`:
```csharp
app.UseAuthentication();
app.UseCors();
app.UseRateLimiter(); // before caching/authorization
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseAuthorization();
```

Rationale:
- For IP-based partitioning, `UseRateLimiter()` can be after CORS. If you switch to user-based partitioning later, ensure it stays after `UseAuthentication()` so `HttpContext.User` is populated.


#### Step 4 — Apply the policy to all controllers

- Apply the named policy using the endpoint convention on controller mappings:
```csharp
app.MapControllers().RequireRateLimiting("fixed-per-minute");
```

This uniformly enforces the policy across all controllers.

---

### Manual Verification

Once the app is running, verify the limiter.

#### Step 1 — Start the API

- Use Rider’s Run button or CLI:
```bash
dotnet run --project Chinook.API
```

Confirm you see startup logs without errors.

#### Step 2 — Make more than 20 requests within a minute

Replace the sample endpoint with any existing controller route. For example, if you have `api/albums`:

macOS/Linux (bash):
```bash
for i in {1..25}; do curl -s -o /dev/null -w "%{http_code}\n" https://localhost:5001/api/albums; done
```

PowerShell:
```powershell
1..25 | ForEach-Object { (Invoke-WebRequest -UseBasicParsing https://localhost:5001/api/albums).StatusCode }
```

Expected:
- First ~20 requests return 2xx/normal responses.
- Requests beyond 20 within 60 seconds return `429` with header `Retry-After: 60` and body `Too many requests. Please retry later.`

You can inspect headers with:
```bash
curl -i https://localhost:5001/api/albums
```
