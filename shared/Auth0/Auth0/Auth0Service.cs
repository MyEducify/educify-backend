using Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Auth0.Auth0
{
    public class Auth0Service : IAuth0Service
    {
        private readonly ICacheService _cache;
        IConfiguration _config;
        public Auth0Service(ICacheService cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
        }

        public async Task<bool> IsEmailVerifiedFromAuth0(string email)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetManagementApiToken());
            var res = await client.GetAsync(
                   $"https://{_config["Auth0:Domain"]}/api/v2/users-by-email?email={email}");

            if (!res.IsSuccessStatusCode)
                return false;

            var json = await res.Content.ReadAsStringAsync();
            var users = Utils.ObjectUtil.Utility.DeserializeData<JsonElement>(json);

            // Auth0 returns an array of users; check if it's non-empty
            return users.ValueKind == JsonValueKind.Array && users.GetArrayLength() > 0;
        }

        public Task<bool> ValidateJwtAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult(false);

            try
            {
                token = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7)
                    : token;

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Secret"])
                    ),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParams, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }

        }

        private async Task<string> GetManagementApiToken()
        {
            var m2mtoken = _cache.GetData<string>("Auth0:AccessToken");
            if (m2mtoken != null)
            {
                return m2mtoken;
            }
            using var client = new HttpClient();
            var body = new Dictionary<string, string>
            {
                {"client_id", _config["Auth0:ClientId"]},
                {"client_secret", _config["Auth0:ClientSecret"]},
                {"audience", _config["Auth0:Audience"]},
                {"grant_type", "client_credentials"}
            };

            var res = await client.PostAsync($"https://{_config["Auth0:Domain"]}/oauth/token",
                new FormUrlEncodedContent(body));

            var json = await res.Content.ReadAsStringAsync();
            var token = Utils.ObjectUtil.Utility.DeserializeData<JsonElement>(json);
            _cache.SetData<string>("Auth0:AccessToken", token.GetProperty("access_token").GetString());
            return token.GetProperty("access_token").GetString();
        }
    }
}
