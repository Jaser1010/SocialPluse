using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class Follow
	{
		public Guid FollowerId { get; set; } // composite PK with FolloweeId
		public Guid FolloweeId { get; set; } // composite PK with FollowerId
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
	}
}
