using Auth0.Auth0;
using AuthService.Models.Request;
using AuthService.Models.Response;
using AuthService.Services.Abstract;
using Microservice.Communication.GRPC.UserServiceProtos.Protos;
using Services.JWT;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserGrpcService.UserGrpcServiceClient _client;
        private readonly IAuth0Service _auth0Service;
        private readonly IJWTService _iJWTService;
        public AuthService(UserGrpcService.UserGrpcServiceClient client,
             IAuth0Service auth0Service, IJWTService iJWTService)
        {
            _client = client;
            _auth0Service = auth0Service;
            _iJWTService = iJWTService;
        }

        public async Task<AuthLoginResponse> AuthenticateUser(AuthLoginRequest authLoginRequest)
        {
            try
            {
                if (!await _auth0Service.IsEmailVerifiedFromAuth0(authLoginRequest.Email))
                    throw new ApiException("Email is not verified.", 401, "EMAIL_NOT_VERIFIED");

                var request = new CreateUserRequest
                {
                    Auth0Id = authLoginRequest.Sub,
                    Email = authLoginRequest.Email,
                    FullName = authLoginRequest.Name ?? "",
                    Nickname = authLoginRequest.Nickname ?? "",
                    PictureUrl = authLoginRequest.Picture ?? "",
                    EmailVerified = authLoginRequest.EmailVerified,
                };
                var response = await _client.CreateUserAsync(request);

                var internalToken = _iJWTService.GenerateInternalJwtToken(response);

                return new AuthLoginResponse()
                {
                    AccessToken = $"Bearer {internalToken}",
                    UserId = response.UserId,
                    Name = response.FullName
                };

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
