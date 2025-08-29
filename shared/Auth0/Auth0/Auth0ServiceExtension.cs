using Microservice.Communication.GRPC.AuthServiceProtos.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth0.Auth0
{
    public static class Auth0ServiceExtension
    {
        public static IServiceCollection AddAuth0ServiceExtension(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IAuth0Service, Auth0Service>();

            return services;
        }
    }
}
