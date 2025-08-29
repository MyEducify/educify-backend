namespace AuthService.Models.Response
{
    public class AuthLoginResponse
    {
        public string AccessToken { get; init; }
        public string UserId { get; init; }
        public string Name { get; init; }
    }
}
