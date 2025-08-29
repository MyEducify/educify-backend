using Microsoft.Extensions.DependencyInjection;
using Microservice.Communication.GRPC.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microservice.Communication.GRPC.UserServiceProtos.Protos;
using Microservice.Communication.GRPC.AuthServiceProtos.Protos;

namespace Microservice.Communications.Extensions;

public static class UserServiceGrpcClientExtensions
{
    public static IServiceCollection AddGrpcUserServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["Grpc:AuthService"];
        services.AddGrpcClient<UserAuthGrpcService.UserAuthGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["Grpc:AuthService"]);
        });

        return services;
    }
}
