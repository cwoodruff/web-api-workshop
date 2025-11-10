---
order: 2
icon: rocket
---
# Using Hypermedia in your API Responses

## Phase 1 - Align on Conventions and Scope

- Confirm hypermedia conventions
  - Links container: keep a top‑level `Links` array on each ApiModel via `Representation`/`BaseApiModel`.
  - Rel names: standardize on semantic rels — `self`, `artist`, `album`, `tracks`, `collection`, `next`, `prev`, `first`, `last`.
  - URL generation: always use `LinkGenerator.GetUriByName("RouteName", new { ... })` with named routes.

- Define initial resource scope
  - Start with: Album, Artist, Track. Then expand to Customer, Employee, Genre, MediaType, Playlist, Invoice, InvoiceLine.

- Output shape
   - Keep current DTO shape with `links` array. Titles/Names in links are 
     optional but recommended.

---

## Phase 2 - Foundation Audit and Core Types

### Step 1 Ensure models inherit `BaseApiModel`
- Confirm each ApiModel inherits `BaseApiModel` so `Links` exists.

Code: base representation and link types
```csharp
namespace Chinook.Domain.Helpers
{
    public sealed class Link
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string? Title { get; set; }
        // Optional affordances (enable later if you want):
        public string? Method { get; set; }
        public string? Type { get; set; }
    }

    public interface IRepresentation
    {
        List<Link> Links { get; }
        void AddLink(Link link);
    }

    public abstract class Representation : IRepresentation
    {
        public List<Link> Links { get; } = new();

        // De-duplicate by Rel (case-insensitive). Replace existing if same rel.
        public void AddLink(Link link)
        {
            var i = Links.FindIndex(l => string.Equals(l.Rel, link.Rel, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) Links[i] = link; else Links.Add(link);
        }
    }
}
```

```csharp
namespace Chinook.Domain.ApiModels
{
    using Chinook.Domain.Helpers;

    public abstract class BaseApiModel : Representation
    {
        public int Id { get; set; }
    }
}
```

### Step 2 Name all routes used by links
- Add `Name = "..."` to controller actions for single and collection endpoints (and related endpoints like “album tracks”).

Code: controller route naming examples
```csharp
namespace Chinook.API.Controllers
{
    using Chinook.Domain.ApiModels;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public sealed class TrackController : ControllerBase
    {
        [HttpGet("{id}", Name = "GetTrackById")]
        public ActionResult<TrackApiModel> GetById(int id)
        {
            var model = new TrackApiModel { Id = id, Name = $"Track {id}", AlbumId = 9, GenreId = 3, MediaTypeId = 1 };
            return Ok(model);
        }

        [HttpGet(Name = "GetTracks")]
        public ActionResult<List<TrackApiModel>> GetAll()
        {
            var items = Enumerable.Range(1, 3).Select(i => new TrackApiModel { Id = i, Name = $"Track {i}", AlbumId = 9, GenreId = 3, MediaTypeId = 1 }).ToList();
            return Ok(items);
        }
    }
}
```

### Step 3 Ensure controllers return ApiModels in an ObjectResult
- Return `ActionResult<TApiModel>` or `ActionResult<List<TApiModel>>`. The global result filter enriches `ObjectResult` values.

### Step 4 Confirm pipeline registration
- Register the hypermedia services and the global result filter in DI.

Code: enricher infrastructure and result filter
```csharp
namespace Chinook.Domain.Hypermedia
{
    using Chinook.Domain.Helpers;

    public interface IEnricher
    {
        bool Match(object? value);
        Task ProcessAsync(object? value, CancellationToken ct = default);
    }

    public abstract class Enricher<T> : IEnricher where T : class, IRepresentation
    {
        public virtual bool Match(object? value) => value is T;
        public async Task ProcessAsync(object? value, CancellationToken ct = default)
        {
            if (value is T typed) await ProcessAsync(typed, ct);
        }
        protected abstract Task ProcessAsync(T representation, CancellationToken ct);
    }

    public abstract class ListEnricher<TList> : IEnricher where TList : class
    {
        public virtual bool Match(object? value) => value is TList;
        public async Task ProcessAsync(object? value, CancellationToken ct = default)
        {
            if (value is TList typed) await ProcessAsync(typed, ct);
        }
        protected abstract Task ProcessAsync(TList representation, CancellationToken ct);
    }
}
```

```csharp
namespace Chinook.Domain.Hypermedia
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public sealed class RepresentationEnricher : IAsyncResultFilter
    {
        private readonly IEnumerable<IEnricher> _enrichers;
        public RepresentationEnricher(IEnumerable<IEnricher> enrichers) => _enrichers = enrichers;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
            {
                var value = objectResult.Value;
                foreach (var enricher in _enrichers)
                {
                    if (enricher.Match(value))
                    {
                        await enricher.ProcessAsync(value, context.HttpContext.RequestAborted);
                    }
                }
            }
            await next();
        }
    }
}
```

Code: DI registration and filter wiring
```csharp
namespace Chinook.API.Configurations
{
    using Chinook.Domain.Hypermedia;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServicesConfiguration
    {
        public static IServiceCollection AddHypermedia(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // Register resource enrichers (single + list). Expand as you add more.
            services.AddScoped<IEnricher, TrackEnricher>();
            services.AddScoped<IEnricher, TracksEnricher>();

            services.AddScoped<IEnricher, AlbumEnricher>();
            services.AddScoped<IEnricher, AlbumsEnricher>();

            services.AddScoped<RepresentationEnricher>();
            return services;
        }
    }
}
```

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHypermedia();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Chinook.Domain.Hypermedia.RepresentationEnricher>();
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## Phase 3 - Implement/Refine Enrichers Per Resource

### Step 5 Track enrichers (single + list)

Code: API models (if not present)
```csharp
namespace Chinook.Domain.ApiModels
{
    using Chinook.Domain.Helpers;

    public sealed class TrackApiModel : BaseApiModel
    {
        public string Name { get; set; } = string.Empty;
        public int? AlbumId { get; set; }
        public int? GenreId { get; set; }
        public int? MediaTypeId { get; set; }
    }

    public sealed class AlbumApiModel : BaseApiModel
    {
        public string Title { get; set; } = string.Empty;
        public int? ArtistId { get; set; }
    }
}
```

Code: Track single‑resource enricher
```csharp
namespace Chinook.API.Hypermedia.Enrichers
{
    using Chinook.Domain.ApiModels;
    using Chinook.Domain.Helpers;
    using Chinook.Domain.Hypermedia;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public sealed class TrackEnricher : Enricher<TrackApiModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public TrackEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        protected override Task ProcessAsync(TrackApiModel model, CancellationToken ct)
        {
            var http = _httpContextAccessor.HttpContext!;

            var self = _linkGenerator.GetUriByName(http, "GetTrackById", new { id = model.Id });
            if (!string.IsNullOrEmpty(self))
                model.AddLink(new Link { Rel = "self", Title = $"Track #{model.Id}", Href = self });

            if (model.AlbumId is int albumId)
            {
                var albumHref = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = albumId });
                if (!string.IsNullOrEmpty(albumHref))
                    model.AddLink(new Link { Rel = "album", Title = $"Album #{albumId}", Href = albumHref });
            }

            if (model.GenreId is int genreId)
            {
                var genreHref = _linkGenerator.GetUriByName(http, "GetGenreById", new { id = genreId });
                if (!string.IsNullOrEmpty(genreHref))
                    model.AddLink(new Link { Rel = "genre", Title = $"Genre #{genreId}", Href = genreHref });
            }

            if (model.MediaTypeId is int mediaTypeId)
            {
                var mediaHref = _linkGenerator.GetUriByName(http, "GetMediaTypeById", new { id = mediaTypeId });
                if (!string.IsNullOrEmpty(mediaHref))
                    model.AddLink(new Link { Rel = "mediaType", Title = $"Media Type #{mediaTypeId}", Href = mediaHref });
            }

            return Task.CompletedTask;
        }
    }
}
```

Code: Tracks collection enricher
```csharp
namespace Chinook.API.Hypermedia.Enrichers
{
    using Chinook.Domain.ApiModels;
    using Chinook.Domain.Helpers;
    using Chinook.Domain.Hypermedia;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public sealed class TracksEnricher : ListEnricher<List<TrackApiModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public TracksEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        protected override Task ProcessAsync(List<TrackApiModel> models, CancellationToken ct)
        {
            var http = _httpContextAccessor.HttpContext!;
            foreach (var model in models)
            {
                var self = _linkGenerator.GetUriByName(http, "GetTrackById", new { id = model.Id });
                if (!string.IsNullOrEmpty(self))
                    model.AddLink(new Link { Rel = "self", Title = $"Track #{model.Id}", Href = self });
            }
            return Task.CompletedTask;
        }
    }
}
```

### Step 6 Album enrichers (single + list)

Code: Album single‑resource enricher
```csharp
namespace Chinook.API.Hypermedia.Enrichers
{
    using Chinook.Domain.ApiModels;
    using Chinook.Domain.Helpers;
    using Chinook.Domain.Hypermedia;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public sealed class AlbumEnricher : Enricher<AlbumApiModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public AlbumEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        protected override Task ProcessAsync(AlbumApiModel model, CancellationToken ct)
        {
            var http = _httpContextAccessor.HttpContext!;

            var self = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = model.Id });
            if (!string.IsNullOrEmpty(self))
                model.AddLink(new Link { Rel = "self", Title = $"Album #{model.Id}", Href = self });

            if (model.ArtistId is int artistId)
            {
                var artistHref = _linkGenerator.GetUriByName(http, "GetArtistById", new { id = artistId });
                if (!string.IsNullOrEmpty(artistHref))
                    model.AddLink(new Link { Rel = "artist", Title = $"Artist #{artistId}", Href = artistHref });
            }

            var tracksHref = _linkGenerator.GetUriByName(http, "GetAlbumTracks", new { id = model.Id });
            if (!string.IsNullOrEmpty(tracksHref))
                model.AddLink(new Link { Rel = "tracks", Title = $"Album #{model.Id} Tracks", Href = tracksHref });

            return Task.CompletedTask;
        }
    }
}
```

Code: Albums collection enricher
```csharp
namespace Chinook.API.Hypermedia.Enrichers
{
    using Chinook.Domain.ApiModels;
    using Chinook.Domain.Helpers;
    using Chinook.Domain.Hypermedia;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public sealed class AlbumsEnricher : ListEnricher<List<AlbumApiModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public AlbumsEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        protected override Task ProcessAsync(List<AlbumApiModel> models, CancellationToken ct)
        {
            var http = _httpContextAccessor.HttpContext!;
            foreach (var model in models)
            {
                var self = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = model.Id });
                if (!string.IsNullOrEmpty(self))
                    model.AddLink(new Link { Rel = "self", Title = $"Album #{model.Id}", Href = self });
            }
            return Task.CompletedTask;
        }
    }
}
```

### Step 7 Register enrichers in DI
- Ensure each enricher is registered as `IEnricher` in `AddHypermedia()` (see Phase 1, step 7 code).

---

## Phase 4 — Per‑Resource Link Matrix and Checklist

### Step 8 Define link matrices (developer note)
- Album: `self` → `GetAlbumById`; `artist` → `GetArtistById`; `tracks` → `GetAlbumTracks`.
- Artist: `self` → `GetArtistById`; `albums` → `GetArtistAlbums` or item links via `GetAlbumById`.
- Track: `self` → `GetTrackById`; `album` → `GetAlbumById`; `genre` → `GetGenreById`; `mediaType` → `GetMediaTypeById`.
- Repeat similarly for other resources.

### Step 9 Apply the checklist per resource
- ApiModel inherits `BaseApiModel` and includes `Id`.
- Controller actions have named routes.
- Single enricher adds `self` + relationship links.
- List enricher adds at least per‑item `self` (and pagination links if applicable).
- Enrichers registered in DI; global filter enabled.

---

## Phase 5 — Verification and Tests

### Step 10 Manual verification (curl/Postman)
- `curl -s https://localhost:<port>/api/tracks/42 | jq '.'` and inspect `links`.

### Step 11 Integration test example

Code: minimal integration test
```csharp
using System.Net.Http.Json;
using Chinook.Domain.ApiModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class HypermediaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public HypermediaTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetTrackById_IncludesSelfLink()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/track/42");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<TrackApiModel>();
        Assert.NotNull(json);
        Assert.Contains(json!.Links, l => l.Rel == "self" && !string.IsNullOrWhiteSpace(l.Href));
    }
}
```

### Step 12 Regression guard
- Keep a test asserting the `self` link correctness for a known id, ensuring stability across route template changes (thanks to `GetUriByName`).

### Step 13 Performance considerations
- For large lists, measure impact. Consider future opt‑out via `?includeLinks=false` if needed.

---

## Phase 6 — Optional Refinements (Staying on Current Infra)

### Step 14 Standardize rels
- If any enrichers use numeric rels, migrate to semantic rels (`self`, `artist`, etc.). Optionally publish both during transition.

### Step 15 Nested model links
- If nested models are embedded in responses, add links to them as well (e.g., embedded `ArtistApiModel` inside an `AlbumApiModel`).

### Step 16 Pagination links (if paging is available)

Code: paginated wrapper and enricher
```csharp
namespace Chinook.Domain.ApiModels
{
    using Chinook.Domain.Helpers;

    public sealed class PagedResult<T> : Representation where T : IRepresentation
    {
        public List<T> Items { get; init; } = new();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
    }
}
```

```csharp
namespace Chinook.API.Hypermedia.Enrichers
{
    using Chinook.Domain.ApiModels;
    using Chinook.Domain.Helpers;
    using Chinook.Domain.Hypermedia;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public sealed class PagedTracksEnricher : Enricher<PagedResult<TrackApiModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public PagedTracksEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        protected override Task ProcessAsync(PagedResult<TrackApiModel> model, CancellationToken ct)
        {
            var http = _httpContextAccessor.HttpContext!;

            string? PageUrl(int page) => _linkGenerator.GetUriByName(http, "GetTracks", new { page, pageSize = model.PageSize });

            var self = PageUrl(model.PageNumber);
            if (!string.IsNullOrEmpty(self)) model.AddLink(new Link { Rel = "self", Href = self });

            if (model.PageNumber > 1)
            {
                var prev = PageUrl(model.PageNumber - 1);
                if (!string.IsNullOrEmpty(prev)) model.AddLink(new Link { Rel = "prev", Href = prev });
                var first = PageUrl(1);
                if (!string.IsNullOrEmpty(first)) model.AddLink(new Link { Rel = "first", Href = first });
            }

            if (model.PageNumber < model.TotalPages)
            {
                var next = PageUrl(model.PageNumber + 1);
                if (!string.IsNullOrEmpty(next)) model.AddLink(new Link { Rel = "next", Href = next });
                var last = PageUrl(model.TotalPages);
                if (!string.IsNullOrEmpty(last)) model.AddLink(new Link { Rel = "last", Href = last });
            }

            foreach (var item in model.Items)
            {
                var selfItem = _linkGenerator.GetUriByName(http, "GetTrackById", new { id = item.Id });
                if (!string.IsNullOrEmpty(selfItem))
                    item.AddLink(new Link { Rel = "self", Title = $"Track #{item.Id}", Href = selfItem });
            }

            return Task.CompletedTask;
        }
    }
}
```

### Step 17 Swagger/OpenAPI notes
- Document the `links` array in response schemas and provide examples so clients expect hypermedia.

---

## Phase 7 — Advanced Upgrade Path (Optional, Standards‑Based)

If you later choose to adopt HAL with content negotiation and policy‑based composition:

- Use `_links` and `_embedded`, media type `application/hal+json`.
- Introduce `IHypermediaPolicy<T>` and a `HypermediaComposer` instead of scanning all enrichers.
- Add an output formatter (`HalOutputFormatter`) that writes HAL only when requested via `Accept` header.

Code: basic (placeholder) HAL output formatter skeleton
```csharp
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

public sealed class HalOutputFormatter : TextOutputFormatter
{
    public HalOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/hal+json"));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => true; // tighten for your types

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        // Map Representation.Links => _links as needed (omitted for brevity)
        var json = JsonSerializer.Serialize(context.Object, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        await context.HttpContext.Response.WriteAsync(json, selectedEncoding);
    }
}
```

Wire it when ready:
```csharp
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Insert(0, new HalOutputFormatter());
});
```

---**

## Quick Reference Checklist (Per Endpoint)

- ApiModel inherits `BaseApiModel` and has `Id`.
- Controller action has a unique named route.
- Single‑resource enricher produces `self` + relationship links.
- Collection enricher ensures per‑item `self` and optional pagination links.
- Enrichers are registered via `AddHypermedia()`; global `RepresentationEnricher` filter is enabled.
- Manual and integration tests verify presence and correctness of links.
