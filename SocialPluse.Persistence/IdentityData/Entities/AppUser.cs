using Microsoft.AspNetCore.Identity;

namespace SocialPluse.Persistence.IdentityData.Entities
{
	public class AppUser : IdentityUser<Guid>
	{
		public string? DisplayName { get; set; }
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
