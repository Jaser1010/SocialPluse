using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Posts
{
	public class FeedRequest
	{
		public DateTime? Cursor { get; set; }
		public int Limit { get; set; }
	}
}
