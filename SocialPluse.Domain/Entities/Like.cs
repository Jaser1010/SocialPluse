using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class Like
	{
		public Guid UserId { get; set; } // composite PK with PostId
		public Guid PostId { get; set; } // composite PK with UserId
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
