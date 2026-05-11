using FluentValidation;
using SocialPluse.Shared.DTOs.Comments;

namespace SocialPluse.Shared.Validators
{
	public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
	{
		public CreateCommentRequestValidator()
		{
			RuleFor(x => x.Text)
				.NotEmpty().WithMessage("Comment cannot be empty.")
				.MaximumLength(280).WithMessage("Comment cannot exceed 280 characters.");
		}
	}
}