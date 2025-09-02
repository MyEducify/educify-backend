using Microsoft.Extensions.DependencyInjection;
using Services.JWT;

namespace Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {


            services.AddScoped<IJWTService, JWTService>();
            return services;
        }
    }
}
