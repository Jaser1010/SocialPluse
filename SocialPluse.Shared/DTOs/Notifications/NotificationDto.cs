using SocialPluse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Notifications
{
	public class NotificationDto
	{
		public Guid Id { get; set; }
		public Guid ActorUserId { get; set; }
		public string ActorUsername { get; set; } = default!;
		public NotificationType Type { get; set; }
		public Guid? PostId { get; set; }
		public Guid? CommentId { get; set; }
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
