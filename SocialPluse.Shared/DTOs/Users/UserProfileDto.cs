using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Users
{
	public class UserProfileDto
	{
		public string Id { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Email { get; set; } = default!;
		public string? DisplayName { get; set; }
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }
		public int PostsCount { get; set; }
		public int FollowersCount { get; set; }
		public int FollowingCount { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
