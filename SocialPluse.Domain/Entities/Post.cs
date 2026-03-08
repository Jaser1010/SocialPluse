using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class Post
	{
		public Guid Id { get; set; }
		public Guid AuthorId  { get; set; } // FK => AppUser
		public string Text { get; set; } = default!;
		public string? MediaUrl { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	}
}
