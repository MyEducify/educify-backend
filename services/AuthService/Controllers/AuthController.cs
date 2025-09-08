using AuthService.Models.Request;
using AuthService.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLoginRequest authLoginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    throw new ApiException("Validation failed.", 400, "VALIDATION_ERROR", errors);
                }
                var response = await _authService.AuthenticateUser(authLoginRequest);
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AuthLogin");
                throw;
            }
        }
    }
}
