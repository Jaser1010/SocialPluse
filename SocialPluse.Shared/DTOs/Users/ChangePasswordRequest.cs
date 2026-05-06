namespace SocialPluse.Shared.DTOs.Users
{
	public class ChangePasswordRequest
	{
		public string CurrentPassword { get; set; } = default!;
		public string NewPassword { get; set; } = default!;
	}
}
