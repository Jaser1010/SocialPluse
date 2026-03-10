using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Comments
{
	public class CommentFeedResponse
	{
		public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
		public DateTime? NextCursor { get; set; }

	}
}
