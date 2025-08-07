using Microsoft.Extensions.DependencyInjection;
using Microservice.Communication.GRPC.Protos;
using Microsoft.Extensions.Configuration;

namespace Microservice.Communications.GRPC;

public static class GrpcClientExtensions
{
    public static IServiceCollection AddGrpcServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        var userServiceUrl = configuration["Grpc:UserService"];
        if (string.IsNullOrEmpty(userServiceUrl))
            throw new ArgumentNullException(nameof(userServiceUrl), "UserService gRPC URL is not configured.");

        services.AddGrpcClient<UserService.UserServiceClient>(options =>
        {
            options.Address = new Uri(userServiceUrl);
        });

        return services;
    }
}
