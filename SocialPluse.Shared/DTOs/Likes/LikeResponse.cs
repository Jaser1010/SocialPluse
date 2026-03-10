using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Likes
{
	public class LikeResponse
	{
		public Guid UserId { get; set; }
		public Guid PostId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
