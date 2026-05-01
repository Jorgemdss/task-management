namespace TaskManagement.Application.Interfaces;

public interface ICachedService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync<T>(string key, CancellationToken ct = default);
    Task<T?> GetOrAddAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    );
}
