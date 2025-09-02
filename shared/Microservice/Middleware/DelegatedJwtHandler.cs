using Cache;
using Microservice.Communication.GRPC.AuthServiceProtos.Protos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class DelegatedJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly UserAuthGrpcService.UserAuthGrpcServiceClient _authClient;

    public DelegatedJwtHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IServiceScopeFactory scopeFactory,
        UserAuthGrpcService.UserAuthGrpcServiceClient authClient)
        : base(options, logger, encoder, clock)
    {
        _scopeFactory = scopeFactory;
        _authClient = authClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var path = Request.Path.Value?.ToLower();

        // Bypass authentication for certain routes
        if (!string.IsNullOrEmpty(path) && path.Contains("/user.usergrpcservice/createuser"))
        {
            // Return success with empty principal
            var anonymousIdentity = new ClaimsIdentity();
            var anonymousPrincipal = new ClaimsPrincipal(anonymousIdentity);
            var ticket = new AuthenticationTicket(anonymousPrincipal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return AuthenticateResult.Fail("Missing or invalid Authorization header.");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Extract userId from token (unvalidated)
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return AuthenticateResult.Fail("Invalid token payload.");
        }

        // Redis check
        using var scope = _scopeFactory.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var cacheKey = $"auth_token:{userId}";
        var cachedToken = cache.GetData<string>(cacheKey);

        if (string.IsNullOrEmpty(cachedToken) || cachedToken != token)
        {
            return AuthenticateResult.Fail("Token not found in cache.");
        }

        // gRPC validation with AuthService
        var grpcResponse = await _authClient.ValidateTokenAsync(new ValidateTokenRequest { Token = token });
        if (!grpcResponse.IsValid)
        {
            return AuthenticateResult.Fail("Token validation failed.");
        }

        // Create ClaimsPrincipal
        var claims = jwt.Claims.ToList();
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticketAuth = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticketAuth);
    }
}
