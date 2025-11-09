---
order: 1
icon: gear
---
# Documenting your API with OpenAPI

## Overview
This plan guides a developer to add OpenAPI/Swagger using Swashbuckle to the ASP.NET Core Web API, integrating cleanly with existing API Versioning and Identity/JWT authentication. It includes:
- A high-level overview (what we’re adding and why)
- A detailed, step-by-step implementation plan (exact actions and code snippets)
- A final verification checklist (how to test that everything works)

Note: Where you see “OpenAI” it likely means “OpenAPI.” The plan below assumes OpenAPI via Swashbuckle.

---

## Developer Plan Overview
1) Add Swashbuckle packages to the API project.
2) Enable XML documentation in the API project to surface controller/action summaries in Swagger.
3) Register SwaggerGen:
    - Generate one OpenAPI document per API version using `IApiVersionDescriptionProvider`.
    - Include XML comments.
    - Add JWT Bearer security definition and global requirement.
    - (Optional but recommended) Add operation filters for `[Authorize]`/`[AllowAnonymous]` and ProblemDetails responses.
4) Add Swagger middleware and UI:
    - Add `UseSwagger` and `UseSwaggerUI`.
    - Configure UI endpoints for each discovered API version.
    - (Optional) Lock down Swagger in production or move it to `/docs`.
5) Annotate controllers/actions (incremental polish):
    - Summaries, response types, tags, and version attributes.
6) CI/CD and production:
    - Ensure XML docs are produced in CI.
    - (Optional) Publish versioned `swagger.json` artifacts, lock UI in prod.

---

## Step-by-Step Implementation (Swashbuckle)
These steps assume your API project is `Chinook.API` and minimal-hosting `Program.cs` is used. Adjust paths/names as needed. This is an implementation guide; do not apply changes unless you are ready to implement.

### Step 1 Add NuGet packages
- Swashbuckle core:
    - `Swashbuckle.AspNetCore`
- Optional annotations (recommended for `SwaggerOperation` attributes):
    - `Swashbuckle.AspNetCore.Annotations`
- Command examples:
    - `dotnet add Chinook.API package Swashbuckle.AspNetCore`
    - `dotnet add Chinook.API package Swashbuckle.AspNetCore.Annotations`

### Step 2 Enable XML documentation in the API project
- Edit `Chinook.API.csproj` and ensure:
    - `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
    - Optionally suppress missing XML doc warnings initially: `<NoWarn>$(NoWarn);1591</NoWarn>`
- Confirm the XML output file path (implicitly generated under `bin/<Config>/<TFM>/Chinook.API.xml`). If you want a fixed path, set `<DocumentationFile>bin\$(Configuration)\$(TargetFramework)\Chinook.API.xml</DocumentationFile>`.

### Step 3 Register SwaggerGen in `Program.cs`
- Add the service registration after API Versioning and VersionedApiExplorer registrations.
- Example configuration (illustrative):
```csharp
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

builder.Services.AddSwaggerGen(c =>
{
    // 3.1 XML comments
    var xmlFile = "Chinook.API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // 3.2 JWT Bearer security
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your JWT}'"
    };
    c.AddSecurityDefinition("Bearer", bearerScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });

    // 3.3 Versioned docs: defer to options setup using IApiVersionDescriptionProvider below
});

// After building the app, we will configure Swagger docs per version (see Step 4.2 UI and Step 3.4 docs per version below).
```

### Step 4 Generate one Swagger doc per API version
- Add a post-configure block that uses `IApiVersionDescriptionProvider` to add a `SwaggerDoc` for each API version. Two common approaches:
    - A) Configure inside `AddSwaggerGen` using a transient resolve of `IApiVersionDescriptionProvider` (requires building a temporary provider in `Program.cs`), or
    - B) Use `AddSwaggerGen` as above, and add `ConfigureOptions` pattern to inject API version descriptions.
- Simple in-place pattern:
```csharp
builder.Services.AddSwaggerGen(c => { /* previous config */ });

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// where ConfigureSwaggerOptions is defined as:
public class ConfigureSwaggerOptions : Microsoft.Extensions.Options.IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => _provider = provider;

    public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Chinook API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? "This API version has been deprecated."
                    : "Chinook Web API documentation."
            });
        }
    }
}
```

### Step 5 Add optional operation filters
- `AuthorizeOperationFilter` to reflect `[Authorize]`/`[AllowAnonymous]`, and add 401/403 responses:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType!.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
        var allowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous)
        {
            return; // no security requirement
        }

        if (hasAuthorize)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}
```
- Register the filter:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    // ...existing config...
    c.OperationFilter<AuthorizeOperationFilter>();
});
```
- Optional `ProblemDetailsOperationFilter` to add `application/problem+json` for error responses.

### Step 6 Add middleware and UI (after building the app)
- Place after authentication/authorization is configured but before `MapControllers()` is also acceptable. Commonly used before mapping endpoints.
```csharp
var app = builder.Build();

// ... existing middleware: CORS, caching, HTTPS, authz, etc.

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                $"Chinook API {description.GroupName.ToUpperInvariant()}");
    }
    // Optional: place docs at /docs instead of /swagger
    // options.RoutePrefix = "docs";

    // Optional: disable Try it out in production
    // options.SupportedSubmitMethods(Array.Empty<Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod>());
});

app.MapControllers();
app.Run();
```

### Step 7 Annotations and controller polish (incremental)
- Add summaries/remarks via XML doc comments on controllers/actions.
- Use `SwaggerOperation` for concise summaries, descriptions, operationId, and tags.
- Add `ProducesResponseType` for all expected outcomes (e.g., `200/201/204`, `400`, `401`, `403`, `404`, `409`, `422`, `500`).
- Ensure version attributes are correct: `[ApiVersion("1.0")]`, `[ApiVersion("2.0")]`, and `MapToApiVersion` for per-action splits.
- Confirm route templates include version segment if using `UrlSegmentApiVersionReader` (e.g., `api/v{version:apiVersion}/[controller]`).

---

### Verification Overview and Checklist
Use this checklist to confirm correct behavior after implementation.

Functional UI checks
- Swagger UI accessible at `/swagger`.
- Multiple API versions appear (e.g., `v1`, `v2`) if your app exposes more than one.
- Selecting each version shows only the endpoints for that version.
- XML summaries from controllers/actions are visible in operation descriptions.

Security and identity
- `Authorize` button present in Swagger UI with a `Bearer` scheme.
- Calling a secured endpoint without a token returns `401` and UI shows `401` response documentation.
- Calling with a valid JWT lacking `Admin`/`Manager` roles returns `403` for endpoints governed by the `CanWrite` policy.
- Endpoints marked `[AllowAnonymous]` (e.g., token issuance) are callable without setting a token in Swagger UI.

Contracts and responses
- Each endpoint advertises correct `ProducesResponseType` entries for success and errors.
- Error responses use `application/problem+json` (if added via filter) and include `ProblemDetails` fields.
- Content types match expectations (`application/json`, `application/octet-stream` for file downloads, etc.).

Versioning alignment
- Routes include `v{version:apiVersion}` when using `UrlSegmentApiVersionReader`.
- `[ApiVersion]` and `MapToApiVersion` attributes correctly align with routes and displayed versions.
- Deprecated versions are marked as such in the document description if applicable.

Stability and automation
- OpenAPI JSON documents are available at `/swagger/{group}/swagger.json` for all supported versions.
- (Optional) CI builds produce XML docs and publish OpenAPI artifacts.
- (Optional) OpenAPI linter passes (no severe errors).

Acceptance criteria
- A developer can obtain a JWT and call secured endpoints via Swagger UI successfully.
- Anonymous endpoints remain accessible without a token.
- The OpenAPI documents are accurate, versioned, and consumable by tooling for client generation.
