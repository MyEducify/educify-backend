using Microsoft.Extensions.DependencyInjection;
using Microservice.Communication.GRPC.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microservice.Communication.GRPC.UserServiceProtos.Protos;
using Microservice.Communication.GRPC.AuthServiceProtos.Protos;

namespace Microservice.Communications.Extensions;

public static class AuthServiceGrpcClientExtensions
{
    public static IServiceCollection AddGrpcAuthServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<UserService.UserServiceClient>(options =>
        {
            options.Address = new Uri(configuration["Grpc:UserService"]);
        });

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Grpc:UserService"]);
        });

        return services;
    }
}
