using FluentValidation;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Shared.Validators
{
	public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
	{
		public CreatePostRequestValidator()
		{
			RuleFor(x => x.Text)
				.NotEmpty().WithMessage("Post text cannot be empty.")
				.MaximumLength(500).WithMessage("Post text cannot exceed 500 characters.");

			RuleFor(x => x.MediaUrl)
				.Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
				.WithMessage("MediaUrl must be a valid absolute URL.");
		}
	}
}