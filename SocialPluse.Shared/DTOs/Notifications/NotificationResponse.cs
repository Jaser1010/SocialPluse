using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Notifications
{
	public class NotificationResponse
	{
		public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
		public DateTime? NextCursor { get; set; }
	}
}
