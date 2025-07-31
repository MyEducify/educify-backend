using Redis;

namespace Cache
{
    public interface ICacheService
    {
        void SetData<T>(string key, T data);
        T? GetData<T>(string key);
        Task RemoveData(string key);
    }
    public class CacheService : ICacheService
    {
        IRedisCacheService _redisCache;
        public CacheService(IRedisCacheService redisCache)
        {
            _redisCache = redisCache;
        }
        public void SetData<T>(string key, T data)
        {
            _redisCache.SetData(key, data);
        }

        public T? GetData<T>(string key)
        {
            return _redisCache.GetData<T>(key);
        }

        public async Task RemoveData(string key)
        {
            await _redisCache.RemoveAsync(key);
        }
    }
}
