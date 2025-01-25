using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AdvancedLINQApiShowcase.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache? _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache? cache, ILogger<CacheService> logger)
        {
            this._cache = cache;
            this._logger = logger;

        }

        public T? GetData<T>(string key)
        {
            var data = _cache?.GetString(key);

            if (data is null)
            {
                _logger.LogWarning("Cache miss for key: {CacheKey}", key);
                return default;
            }

            _logger.LogInformation("Cache hit for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(data);

        }

        public void SetData<T>(string key, T data)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache?.SetString(key, JsonSerializer.Serialize(data), options);
            _logger.LogInformation("Data cached for key: {CacheKey}, expires in {ExpirationMinutes} minutes", key, 5);
        }
    }
}
