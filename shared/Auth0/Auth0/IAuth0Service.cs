using Microservice.Communication.GRPC.UserServiceProtos.Protos;

namespace Auth0.Auth0
{
    public interface IAuth0Service
    {
        Task<bool> IsEmailVerifiedFromAuth0(string email);
        Task<bool> ValidateJwtAsync(string token);
    }
}
