using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class Comment
	{
		public Guid Id { get; set; }
		public Guid PostId { get; set; } // FK => Post
		public Guid AuthorId { get; set; } // FK => AppUser
		public string Text { get; set; } = default!;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
