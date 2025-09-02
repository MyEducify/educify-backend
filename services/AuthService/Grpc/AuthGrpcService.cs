using Auth0.Auth0;
using Grpc.Core;
using Microservice.Communication.GRPC.AuthServiceProtos.Protos;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Grpc
{
    [AllowAnonymous]
    public class AuthGrpcService : UserAuthGrpcService.UserAuthGrpcServiceBase
    {
        private readonly IAuth0Service _auth0Service;
        ILogger<AuthGrpcService> _logger;

        public AuthGrpcService(IAuth0Service auth0Service, ILogger<AuthGrpcService> logger)
        {
            _auth0Service = auth0Service;
            _logger = logger;
        }

        public override async Task<ValidateTokenResponse> ValidateToken(
            ValidateTokenRequest request,
            ServerCallContext context)
        {
            try
            {
                var isValid = await _auth0Service.ValidateJwtAsync(request.Token);
                if (!isValid)
                {
                    return new ValidateTokenResponse { IsValid = false, ExpirySeconds = 0 };
                }

                // Extract expiry from token
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(request.Token);
                var expUnix = long.Parse(jwt.Claims.First(c => c.Type == "exp").Value);
                var expirySeconds = (int)(DateTimeOffset.FromUnixTimeSeconds(expUnix) - DateTimeOffset.UtcNow).TotalSeconds;

                return new ValidateTokenResponse
                {
                    IsValid = true,
                    ExpirySeconds = expirySeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ValidateToken");
                throw;
            }
        }
    }
}
