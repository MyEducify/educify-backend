using Microservice.Communication.GRPC.Protos;

namespace AuthService
{
    public class SomeService
    {
        private readonly UserService.UserServiceClient _userServiceClient;

        public SomeService(UserService.UserServiceClient userServiceClient)
        {
            _userServiceClient = userServiceClient;
        }

        public async Task CallUserGrpc()
        {
            var response = await _userServiceClient.GetUserByIdAsync(new GetUserRequest { UserId = "123" });
            Console.WriteLine($"User: {response.Email}");
        }
    }
}
