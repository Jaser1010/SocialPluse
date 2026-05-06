namespace SocialPluse.Shared.DTOs.Users
{
	public class UserRecommendationDto
	{
		public string Id { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string? DisplayName { get; set; }
		public string? AvatarUrl { get; set; }
	}
}
