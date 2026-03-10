using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Posts
{
	public class PostDto
	{
		public Guid Id { get; set; }
		public Guid AuthorId { get; set; }
		public string AuthorUsername { get; set; } = default!;
		public string Text { get; set; } = default!;
		public string? MediaUrl { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
