using Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Redis
{
    public static class RedisServiceExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "redis_instance:";
            });
            services.AddScoped<IRedisCacheService, RedisCacheService>();

            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }

}
