using Microsoft.Extensions.Caching.Distributed;
using Utils.ObjectUtil;

namespace Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public void SetData<T>(string key, T data, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(20)
            };

            _cache.SetString(key, Utility.SerializeData(data), options);
        }

        public T? GetData<T>(string key)
        {
            var data = _cache.GetString(key);
            if (data is null) return default;
            return Utility.DeserializeData<T>(data);
        }

        public async Task SetDataAsync<T>(string key, T data, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(20)
            };

            await _cache.SetStringAsync(key, Utility.SerializeData(data), options);
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (data is null) return default;
            return Utility.DeserializeData<T>(data);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
