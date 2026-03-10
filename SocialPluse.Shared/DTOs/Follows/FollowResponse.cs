using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Follows
{
	public class FollowResponse
	{
		public Guid FollowerId { get; set; }
		public Guid FolloweeId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
