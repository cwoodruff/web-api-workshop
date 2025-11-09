using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor.Caching;

public class VersionedMemoryCache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, object> _versionLocks = new();

    public VersionedMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string VersionKey(string entity) => $"ver:{entity}";

    public long GetVersion(string entity)
    {
        if (_cache.TryGetValue(VersionKey(entity), out long version))
        {
            return version;
        }
        // initialize to 1
        version = 1;
        _cache.Set(VersionKey(entity), version);
        return version;
    }

    public void BumpVersion(string entity)
    {
        var vlock = _versionLocks.GetOrAdd(entity, _ => new object());
        lock (vlock)
        {
            var current = GetVersion(entity);
            var next = current + 1;
            _cache.Set(VersionKey(entity), next);
        }
    }

    public string EntityById(string entity, int id)
        => $"{entity}:{GetVersion(entity)}:{id}";

    public string ByFk(string entity, string suffix, int id)
        => $"{entity}:{GetVersion(entity)}:{suffix}:{id}";

    public string All(string entity)
        => $"{entity}:{GetVersion(entity)}:all";

    private SemaphoreSlim GetLock(string key)
        => _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, MemoryCacheEntryOptions options)
    {
        if (_cache.TryGetValue(key, out T? existing))
        {
            return existing;
        }

        var sem = GetLock(key);
        await sem.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out existing))
                return existing;

            var created = await factory();
            if (created is not null)
            {
                _cache.Set(key, created, options);
            }
            return created;
        }
        finally
        {
            sem.Release();
        }
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
