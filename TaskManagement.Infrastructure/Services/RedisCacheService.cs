using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class RedisCacheService : ICachedService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        return bytes == null ? default : JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task<T?> GetOrAddAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    )
    {
        var cachedValue = await GetAsync<T>(key, ct);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, ct);
        }

        return value;
    }

    public async Task RemoveAsync<T>(string key, CancellationToken ct = default) =>
        await _cache.RemoveAsync(key, ct);

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    )
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl,
        };

        await _cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), options, ct);
    }
}
