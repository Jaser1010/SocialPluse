using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; } // FK => AppUser
		public string Token { get; set; } = default!;
		public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? RevokedAt { get; set; }
	}
}
