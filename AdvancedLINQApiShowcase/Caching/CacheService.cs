using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AdvancedLINQApiShowcase.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache? _cache;

        public CacheService(IDistributedCache? cache)
        {
            _cache = cache;
        }

        public T? GetData<T>(string key)
        {
            var data = _cache?.GetString(key);

            if (data is null)
                return default(T?);

            return JsonSerializer.Deserialize<T>(data);
                            
        }
        
        public void SetData<T>(string key, T data)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

            _cache?.SetString(key, JsonSerializer.Serialize(data), options);
        }
    }
}
