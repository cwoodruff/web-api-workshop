---
order: 4
icon: gear
---
### Caching Data in Your API

#### START FROM PREVIOUS MODULE'S END
[Developing the Web API Business Rules](api-business-rules.md)

---

### What you will implement in this module
- HTTP response caching for controllers/actions (client/proxy cache hints)
- A unified, robust in-process caching strategy for business-layer reads using `IMemoryCache` via a small helper called `VersionedMemoryCache`
- Reliable invalidation on writes (Create/Update/Delete) and for cross-entity impacts
- Consistent cache keys, TTLs, and stampede protection

This replaces the previous ad-hoc patterns (e.g., `"Album-<id>"` keys, inconsistent expirations, and missing invalidation).

---

### Response Caching (HTTP-level)
Response caching is about client/proxy behavior and is complementary to server-side memory caching.

#### 1) Add response caching to DI
`ServicesConfiguration.AddCaching()`
```csharp
public static void AddCaching(this IServiceCollection services, IConfiguration configuration)
{
    services.AddResponseCaching();
    services.AddMemoryCache();
    // Optional: leave distributed cache registration if you also demo it later
    // services.AddDistributedSqlServerCache(...);
}
```

#### 2) Enable middleware in Program/Startup
Ensure `UseResponseCaching` runs after CORS.
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConnectionProvider(builder.Configuration);
builder.Services.ConfigureRepositories();
builder.Services.ConfigureSupervisor();
builder.Services.AddAPILogging();
builder.Services.AddCORS();
builder.Services.ConfigureValidators();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### 3) Opt-in per controller/action
```csharp
[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[ResponseCache(Duration = 604800)] // 7 days in seconds
public class GenreController : ControllerBase
{
    // ...
}
```
Response header example: `Cache-Control: public,max-age=604800`

Use Postman/browser dev tools to observe the effect. This does not cache on the server; it only instructs clients/proxies.

---

### In-memory caching with VersionedMemoryCache (server-side)
We standardize on an in-process cache-aside approach backed by `IMemoryCache` and a small helper you already have in the project: `Chinook.Domain/Supervisor/Caching/VersionedMemoryCache.cs`.

Key benefits:
- Standardized versioned keys per entity (automatic bulk invalidation)
- Per-key async locking to prevent cache stampedes
- Centralized TTL policies
- Minimal code in `ChinookSupervisor` methods

#### Core concepts
- Key schema (namespaced + versioned):
    - Single entity: `entity:{ver}:{id}` (e.g., `album:3:10`)
    - Lists/FK: `entity:{ver}:{suffix}:{id}` (e.g., `track:5:by-album:10`)
    - All: `entity:{ver}:all`
- Per-entity version: internal counter stored in cache under `ver:{entity}`. Bumping it invalidates all old keys logically.
- Cache-aside flow: Try cache → load repo → set cache with TTL → return.
- Writes must remove by-id and bump the entity version (and sometimes cross-entity versions) to avoid stale lists.

#### Helper API you use in Supervisor
From `VersionedMemoryCache`:
```csharp
// Build logical keys (they include the current version internally)
string EntityById(string entity, int id)
string ByFk(string entity, string suffix, int id)
string All(string entity)

// Get or create with per-key lock
Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, MemoryCacheEntryOptions options)

// Invalidate
void Remove(string key)
void BumpVersion(string entity)
```

#### DI wiring (already present)
- `services.AddMemoryCache()` in `AddCaching`
- `ChinookSupervisor` injects `IMemoryCache` and constructs `_vCache`:
```csharp
private readonly IMemoryCache _cache;
private readonly VersionedMemoryCache _vCache;

public ChinookSupervisor(..., IMemoryCache memoryCache, ...)
{
    _cache = memoryCache;
    _vCache = new VersionedMemoryCache(_cache);
}
```

#### TTL conventions
- Entity by id: Absolute 1 day, Sliding 1 hour
- Large collections (GetAll): 3 minutes
- Lists by FK (by artist/album/genre/etc.): 10 minutes
- Reference/static-like (genre, media type): Absolute 2 days, Sliding 6 hours for by-id; 12 hours for lists

Use `MemoryCacheEntryOptions` accordingly in each method.

---

### How to write Supervisor methods with the new cache
Below are canonical patterns using code that mirrors what’s already implemented in your repo.

#### Reads — Get by id
```csharp
public async Task<AlbumApiModel?> GetAlbumById(int id)
{
    var key = _vCache.EntityById("album", id);
    var options = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromDays(1))
        .SetSlidingExpiration(TimeSpan.FromHours(1));

    return await _vCache.GetOrCreateAsync(key, async () =>
    {
        var album = await _albumRepository.GetById(id);
        if (album == null) return null;
        var model = album.Convert();
        var artist = await GetArtistById(album.ArtistId);
        if (artist != null) model.ArtistName = artist.Name;
        model.Tracks = (await GetTrackByAlbumId(id) ?? Array.Empty<TrackApiModel>()).ToList();
        return model;
    }, options);
}
```

#### Reads — Get all
```csharp
public async Task<IEnumerable<AlbumApiModel>> GetAllAlbum()
{
    var key = _vCache.All("album");
    var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

    return await _vCache.GetOrCreateAsync(key, async () =>
    {
        var albums = await _albumRepository.GetAll();
        return albums.ConvertAll();
    }, options) ?? Array.Empty<AlbumApiModel>();
}
```

#### Reads — List by foreign key
```csharp
public async Task<IEnumerable<TrackApiModel>> GetTrackByAlbumId(int id)
{
    var key = _vCache.ByFk("track", "by-album", id);
    var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
    return await _vCache.GetOrCreateAsync(key, async () =>
    {
        var tracks = await _trackRepository.GetByAlbumId(id);
        return tracks.ConvertAll();
    }, options) ?? Array.Empty<TrackApiModel>();
}
```

#### Writes — Invalidate correctly
On successful update/delete: remove the current by-id item and bump version.
```csharp
public async Task<bool> UpdateAlbum(AlbumApiModel albumApiModel)
{
    // ... map & repo update
    var updated = await _albumRepository.Update(album);
    if (updated)
    {
        _vCache.Remove(_vCache.EntityById("album", album.Id));
        _vCache.BumpVersion("album");
    }
    return updated;
}
```

On create: bump the version (lists need refresh; by-id isn’t cached yet).
```csharp
public async Task<AlbumApiModel> AddAlbum(AlbumApiModel newAlbumApiModel)
{
    // ... validate & repo add
    _vCache.BumpVersion("album");
    return newAlbumApiModel;
}
```

#### Cross-entity invalidation rules (examples)
- Artist changes may impact album views → also bump `album` version when artist updates
- Genre/MediaType updates impact track views → bump `track` version
- Employee changes can impact customer support-rep projections → bump `customer` version
- Customer changes may impact invoice lists → bump `invoice` version

These are already reflected in the Supervisor partials.

---

### Migration guide from the old approach
Replace any of the following patterns:
- Ad-hoc keys like `"Album-" + id` or `"Artist-" + id`
- Misuse of `MemoryCacheEntryOptions` (e.g., passing a `TimeSpan` instead of the options object to `Set`)
- Missing invalidation on writes
- Mixed in-memory and distributed caching for read paths that should remain in-process

With these steps:
1) Ensure `services.AddMemoryCache()` is present.
2) Use `_vCache` in all Supervisor read methods:
    - Build a key with `EntityById`, `All`, or `ByFk`.
    - Call `GetOrCreateAsync` with a factory and `MemoryCacheEntryOptions`.
3) In all write methods (Add/Update/Delete):
    - On update/delete: `_vCache.Remove(_vCache.EntityById("entity", id));`
    - Then `_vCache.BumpVersion("entity");`
    - Add cross-entity bumps as needed.
4) Remove any leftover direct `IMemoryCache.Set/Get` with raw strings.
5) Do not cache `InvoiceLine` and `PlaylistTrack` — intentionally left uncached.

---

### Verification checklist
Manual checks (Postman or similar):
- Cold read (e.g., `GET /api/album/10`) hits DB once and populates cache. Repeated reads return fast.
- Concurrent requests for the same key do not stampede the DB (harder to see, but you can simulate with a small script).
- After an update to an album, the next `GET` misses and repopulates due to version bump.
- Artist rename triggers fresh album views if you also bump `album`.
- Memory pressure/evictions (optional): enable logging on eviction callbacks to observe behavior.

---

### Troubleshooting / FAQ
- Q: I updated an entity, but lists still return old data briefly.
    - A: Ensure you call `_vCache.BumpVersion("entity")` on writes and that readers use versioned keys. Old keys become unreachable; new reads will repopulate.

- Q: Why do I sometimes still see a DB call on a repeated read?
    - A: Entry might have expired (TTL), been evicted under pressure, or the version was bumped due to a related write.

- Q: Can I make TTLs configurable?
    - A: Yes. Wrap TTLs in options loaded from `IConfiguration` or create a central `CachePolicies` static with values bound from `appsettings`.

- Q: Do I need distributed cache?
    - A: Not for this in-process strategy. Distributed cache can be used for cross-instance sharing, but keep the Supervisor’s primary layer as `IMemoryCache` for speed.

---

### Advanced (Optional): Distributed Caching Appendix
If you want to demonstrate SQL Server distributed cache in addition to the in-memory layer, you can keep the existing tooling steps (creating the cache table, connection string, and `services.AddDistributedSqlServerCache`). However, the Supervisor’s current design standardizes on `VersionedMemoryCache` (in-memory). If you later add a 2nd layer, use the same key scheme and invalidation semantics. Keep concerns separated (L1 memory, optional L2 distributed).

Tooling recap (optional):
- Install global tool: `dotnet tool install --global dotnet-sql-cache`
- Create DB/table: `dotnet sql-cache create "<connstring>" dbo ChinookCache`
- Add connection string `ChinookSQLCache` to `appsettings.json`
- Register in `AddCaching` via `services.AddDistributedSqlServerCache(...)`

Do not mix distributed cache calls into the Supervisor read paths unless you explicitly implement a layered cache helper. Stick with `_vCache` for now.

---

### Summary
You now have:
- HTTP response caching for client/proxy hints
- A consistent, versioned, stampede-safe in-memory caching strategy in the Supervisor
- Clear invalidation on writes and cross-entity impacts
- A migration path off legacy patterns and optional guidance for adding a distributed cache later

