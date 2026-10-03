using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private static readonly ConcurrentDictionary<string, bool> _allKeys = new();

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
            };

            options.RegisterPostEvictionCallback((k, _, _, _) =>
            {
                _allKeys.TryRemove(k.ToString()!, out _);
            });

            _memoryCache.Set(key, value, options);
            _allKeys.TryAdd(key, true);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}. Fetching from source...", key);
            var result = await factory();

            if (result != null)
            {
                await SetAsync(key, result, expiration);
            }

            return result;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _allKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache evicted for key: {CacheKey}", key);
        }

        public void RemoveByPrefix(string prefix)
        {
            var keysToRemove = _allKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
            _logger.LogDebug("Cache evicted {Count} keys with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
    }
}
