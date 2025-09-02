using AuthService.Models.Request;
using AuthService.Models.Response;

namespace AuthService.Services.Abstract
{
    public interface IAuthService
    {
        Task<AuthLoginResponse> AuthenticateUser(AuthLoginRequest authLoginRequest);
    }
}
