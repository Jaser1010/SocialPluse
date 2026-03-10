using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Posts
{
	public class FeedResponse
	{
		public List<PostDto> Posts { get; set; } = new List<PostDto>();
		public DateTime? NextCursor { get; set; }
	}
}
