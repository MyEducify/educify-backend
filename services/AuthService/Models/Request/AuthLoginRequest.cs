using FluentValidation;
using System.Text.Json.Serialization;

namespace AuthService.Models.Request
{
    public class AuthLoginRequest
    {
        public string Nickname { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public string Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        public string Sub { get; set; }
    }
    public class AuthLoginRequestValidator : AbstractValidator<AuthLoginRequest>
    {
        public AuthLoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.Sub)
                .NotEmpty().WithMessage("Auth0 user identifier (sub) is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.Nickname)
                .MaximumLength(50).WithMessage("Nickname must not exceed 50 characters.");

            RuleFor(x => x.Picture)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Picture must be a valid URL.");

            RuleFor(x => x.UpdatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("UpdatedAt cannot be in the future.");

            RuleFor(x => x.EmailVerified)
                .NotNull().WithMessage("EmailVerified flag is required.");
        }
    }
}
