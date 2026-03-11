using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Safety
{
	public class BlockResponse
	{
		public Guid BlockerId { get; set; }
		public Guid BlockedId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
