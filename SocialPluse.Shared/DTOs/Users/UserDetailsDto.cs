namespace SocialPluse.Shared.DTOs.Users
{
	public class UserDetailsDto
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? DisplayName { get; set; }
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}