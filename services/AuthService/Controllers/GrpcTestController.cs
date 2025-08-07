using Microservice.Communication.GRPC.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrpcTestController : ControllerBase
    {
        private readonly UserService.UserServiceClient _userServiceClient;

        public GrpcTestController(UserService.UserServiceClient userServiceClient)
        {
            _userServiceClient = userServiceClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var response = await _userServiceClient.GetUserByIdAsync(new GetUserRequest
            {
                UserId = id
            });

            return Ok(response);
        }
    }
}
