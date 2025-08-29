using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Cache;
using Microservice.Communication.GRPC.AuthServiceProtos.Protos;
using Microservice.Communication.GRPC.UserServiceProtos.Protos;
using Services.JWT;

public class JWTService : IJWTService
{
    private readonly IConfiguration _config;
    private readonly ICacheService _cache;

    public JWTService(IConfiguration config, ICacheService cache)
    {
        _config = config;
        _cache = cache;
    }

    public string GenerateInternalJwtToken(CreateUserReply user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.FullName ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        string userAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

        _cache.SetData($"auth_token:{user.UserId}", userAccessToken);

        return userAccessToken;
    }
}
