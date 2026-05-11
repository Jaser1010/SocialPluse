using FluentValidation;
using SocialPluse.Shared.DTOs.Safety;

namespace SocialPluse.Shared.Validators
{
	public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
	{
		public CreateReportRequestValidator()
		{
			RuleFor(x => x.TargetType)
				.Must(x => x.ToLower() == "user" || x.ToLower() == "post")
				.WithMessage("TargetType must be either 'user' or 'post'.");

			RuleFor(x => x.Reason)
				.NotEmpty()
				.Length(5, 200).WithMessage("Reason must be between 5 and 200 characters.");
		}
	}
}