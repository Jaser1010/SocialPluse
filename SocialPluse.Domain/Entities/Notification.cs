using SocialPluse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Domain.Entities
{
	public class Notification
	{
		public Guid Id { get; set; }
		public Guid RecipientUserId { get; set; } // FK => AppUser (the user who receives the notification)
		public Guid ActorUserId { get; set; } // FK => AppUser (the user who triggered the notification)
		public NotificationType Type { get; set; }
		public Guid? PostId { get; set; } // FK => Post
		public Guid? CommentId { get; set; } // FK => Comment
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
