---
order: 4
icon: rocket
---
# Formatting Response Data in your API

## Overview
This plan guides a developer through modern, practical ways to implement and demonstrate response formatting in an ASP.NET Core 10 Web API (controller-based and optional minimal APIs). You’ll:
- Start with the default JSON behavior and explicit string/text responses.
- Enable stricter content negotiation (honor `Accept` and return 406 when unsupported).
- Add extra formatters (XML) only if needed.
- Demonstrate route-based `.{format}` URLs vs. `Accept` negotiation.
- Document responses for OpenAPI/Swagger.
- Show standardized error responses using `ProblemDetails`.
- Optionally add a simple custom formatter (CSV) to illustrate extensibility.
- Verify behavior with `curl`, browser/Postman, and Swagger UI.

All steps are incremental and reversible. Use the smallest possible changes per step and verify as you go.

---

## Detailed Implementation Steps

### Step 1 Prerequisites and baseline
- Ensure your API builds and runs.
- Confirm you have a simple controller returning data, e.g., `AlbumController.Get()` returning `ActionResult<List<AlbumApiModel>>`.
- In `Program.cs` (Chinook.API), you likely have:
```csharp
builder.Services.AddControllers();
app.MapControllers();
```

Why: Establish the baseline behavior — JSON by default via `System.Text.Json`.

---

### Step 2 Return strongly-typed JSON with explicit OpenAPI metadata
- Update one GET action to use `ActionResult<T>` and add per-status metadata.

Example (controller):
```csharp
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AlbumApiModel>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<List<AlbumApiModel>>> Get()
{
    var albums = await _chinookSupervisor.GetAllAlbum();
    return Ok(albums);
}
```

Why: Clear contract for clients; accurate Swagger/OpenAPI generation.

---

### Step 3 Explicit plain text responses (health/version/about)
- Add one endpoint that returns text and explicitly sets `text/plain`.

Controller example:
```csharp
[HttpGet("version")]
[Produces("text/plain")]
public IActionResult Version() => Content("1.0.0", "text/plain");
```

Why: Demonstrates non-JSON content and explicit content type.

---

### Step 4 Enable stricter content negotiation (optional, recommended for demos)
- Honor browser `Accept` and return 406 for unsupported media types.

`Program.cs`:
```csharp
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true; // honor Accept from browsers and clients
    options.ReturnHttpNotAcceptable = true;    // 406 if no formatter matches
});
```

Why: Makes the negotiation behavior visible and testable.

---

### Step 5 Add an extra formatter (XML) only if needed
- If you want to show multiple formats, add XML output support.

`Program.cs` (extend the registration above):
```csharp
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlSerializerFormatters()
.AddJsonOptions(o =>
{
    // optional JSON tuning; keeps example realistic
    o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
```

Why: Demonstrates adding a secondary format without changing endpoints.

---

### Step 6 Demonstrate `Accept`-driven negotiation
- Use the existing GET endpoint; no code changes. Clients control the format via `Accept`:
    - `Accept: application/json` → JSON
    - `Accept: application/xml` → XML (since you added XML)
    - `Accept: text/csv` → 406 (no CSV formatter yet) because `ReturnHttpNotAcceptable = true`

Why: Shows the default, standards-based negotiation path.

---

### Step 7 Demonstrate route-based format with `[FormatFilter]` (legacy/optional)
- If you want URL suffixes like `/api/albums/5.json`:

Controller setup:
```csharp
[ApiController]
[Route("api/[controller]")]
[FormatFilter]
public class AlbumsController : ControllerBase
{
    [HttpGet("{id}.{format?}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlbumApiModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var album = await _chinookSupervisor.GetAlbum(id);
        return album is null ? NotFound() : Ok(album);
    }
}
```

- With XML formatter enabled, the following URLs work (negotiation fallback still applies):
    - `/api/albums/5` → default formatter (typically JSON)
    - `/api/albums/5.json` → JSON
    - `/api/albums/5.xml` → XML

Why: Demonstrates an alternative for clients that prefer URL suffixes.

---

### Step 8 Standardize error responses with `ProblemDetails`
- Add one example action that returns a standardized error.

Example:
```csharp
[HttpGet("fail-example")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public IActionResult FailExample()
{
    return Problem(title: "Bad request", detail: "Example failure", statusCode: 400);
}
```

- With `[ApiController]` enabled, invalid model states automatically return `ValidationProblemDetails` (no extra code).

Why: Shows modern, interoperable error payloads.

---

### Step 9 Optional: Implement a simple custom CSV output formatter
- Create a tiny formatter for demo purposes (e.g., for a flat DTO). This is optional but great for workshops.

Skeleton steps:
1) Create a `CsvOutputFormatter : TextOutputFormatter`.
2) Support `text/csv` media type and UTF-8 encoding.
3) Implement `CanWriteType` and `WriteResponseBodyAsync` to serialize a list of DTOs.
4) Register it:
```csharp
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
    options.OutputFormatters.Add(new CsvOutputFormatter());
});
```

Why: Demonstrates extensibility of the pipeline beyond built-ins.

---

### Step 10 File and stream responses (bonus)
- Add an endpoint returning a file/bytes/stream with explicit content type.

Example:
```csharp
[HttpGet("sample.txt")]
[Produces("text/plain")]
public IActionResult GetSampleFile()
{
    var bytes = System.Text.Encoding.UTF8.GetBytes("Hello file\n");
    return File(bytes, "text/plain", "sample.txt");
}
```

Why: Shows content-type control outside of object serialization.

---

### Follow-up: Verification Checklist and Commands
Use `curl` (or Postman) to verify each behavior. Replace `https://localhost:5001` with your dev URL.

1) Baseline JSON (status 200, `application/json`):
```
curl -i https://localhost:5001/api/albums
```

2) Explicit text response (status 200, `text/plain`):
```
curl -i https://localhost:5001/api/albums/version
```

3) Accept negotiation — JSON (200):
```
curl -i -H "Accept: application/json" https://localhost:5001/api/albums
```

4) Accept negotiation — XML (200 if XML formatter added):
```
curl -i -H "Accept: application/xml" https://localhost:5001/api/albums
```

5) Unsupported Accept (406 when `ReturnHttpNotAcceptable = true`):
```
curl -i -H "Accept: text/csv" https://localhost:5001/api/albums
```

6) Route-based format filter (if enabled):
```
curl -i https://localhost:5001/api/albums/5.json
curl -i https://localhost:5001/api/albums/5.xml
```

7) ProblemDetails error:
```
curl -i https://localhost:5001/api/albums/fail-example
```
Expect `application/problem+json` with a JSON body containing `type`, `title`, `status`, etc.

8) File response:
```
curl -i https://localhost:5001/api/albums/sample.txt
```
Expect `Content-Type: text/plain` and a `Content-Disposition` filename.

9) Swagger/OpenAPI checks:
- Run the app and open Swagger UI.
- Confirm response schemas, content types, and status codes match your attributes and endpoint metadata.
- If you added XML, confirm both `application/json` and `application/xml` are shown for responses.

10) Optional CSV formatter (if implemented):
```
curl -i -H "Accept: text/csv" https://localhost:5001/api/albums
```
Expect `200 OK` and `Content-Type: text/csv` with CSV content.

---

### Tips and Good Practices
- Add only the formatters you will support; let negotiation pick the best match.
- Use `ProducesResponseType` (controllers) or `.Produces<T>()` (minimal) for accurate OpenAPI.
- Prefer `ProblemDetails` for error payloads.
- Be explicit with non-JSON content types (text, files) to avoid ambiguity.
- Keep route `.{format}` only when audiences benefit from URL-based formats; otherwise prefer `Accept` negotiation.
- Consider integration tests that assert `Content-Type` and 406 behavior for key endpoints.
