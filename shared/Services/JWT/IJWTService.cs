using Microservice.Communication.GRPC.UserServiceProtos.Protos;

namespace Services.JWT
{
    public interface IJWTService
    {
        string GenerateInternalJwtToken(CreateUserReply user);
    }
}
