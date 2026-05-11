using FluentValidation;
using SocialPluse.Shared.DTOs.Auth;

namespace SocialPluse.Shared.Validators
{
	public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
	{
		public RegisterRequestValidator()
		{
			RuleFor(x => x.Username)
				.NotEmpty()
				.Length(3, 20).WithMessage("Username must be between 3 and 20 characters.")
				.Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

			RuleFor(x => x.Email).NotEmpty().EmailAddress();

			RuleFor(x => x.Password)
				.NotEmpty()
				.MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
				.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
		}
	}

	public class LoginRequestValidator : AbstractValidator<LoginRequest>
	{
		public LoginRequestValidator()
		{
			RuleFor(x => x.Email).NotEmpty().EmailAddress();
			RuleFor(x => x.Password).NotEmpty();
		}
	}
}