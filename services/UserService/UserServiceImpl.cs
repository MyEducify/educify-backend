using Grpc.Core;
using Microservice.Communication.GRPC.Protos;

namespace UserService
{
    public class UserServiceImpl : Microservice.Communication.GRPC.Protos.UserService.UserServiceBase
    {
        public override Task<UserResponse> GetUserById(GetUserRequest request, ServerCallContext context)
        {
            return Task.FromResult(new UserResponse
            {
                UserId = request.UserId,
                Email = "test@example.com",
                Username = "testuser"
            });
        }
    }

}
