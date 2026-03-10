using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Comments
{
	public class CommentDto
	{
		public Guid Id { get; set; }
		public Guid PostId { get; set; }
		public Guid AuthorId { get; set; }
		public string AuthorUsername { get; set; } = default!;
		public string Text { get; set; } = default!;
		public DateTime CreatedAt { get; set; }
	}
}
