using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public interface IRedisCacheService
    {
        void SetData<T>(string key, T data, TimeSpan? expiry = null);
        T? GetData<T>(string key);
        Task SetDataAsync<T>(string key, T data, TimeSpan? expiry = null);
        Task<T?> GetDataAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
