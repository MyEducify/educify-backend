using AuthService.Models.Request;
using FluentValidation;

namespace AuthService
{
    public static class Extensions
    {
        public static IServiceCollection AddCustomFluentValidation(this IServiceCollection services)
        {
            services.AddTransient<IValidator<AuthLoginRequest>, AuthLoginRequestValidator>();
            return services;
        }

    }
}
