using FluentValidation;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Shared.Validators
{
	public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
	{
		public UpdateProfileRequestValidator()
		{
			RuleFor(x => x.DisplayName)
				.MaximumLength(50).WithMessage("Display name cannot exceed 50 characters.");

			RuleFor(x => x.Bio)
				.MaximumLength(160).WithMessage("Bio cannot exceed 160 characters.");

			RuleFor(x => x.AvatarUrl)
				.Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
				.WithMessage("AvatarUrl must be a valid absolute URL.");
		}
	}
}