using SocialPluse.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface INotificationService
	{
		// Called by Hangfire jobs
		Task CreateFollowNotificationAsync(Guid recipientId, Guid actorId);
		Task CreateLikeNotificationAsync(Guid recipientId, Guid actorId, Guid postId);
		Task CreateCommentNotificationAsync(Guid recipientId, Guid actorId, Guid postId, Guid commentId);
		Task CreateReportNotificationAsync(Guid recipientId, Guid actorId);

		// Called by controller
		Task<NotificationResponse> GetNotificationsAsync(Guid userId, string? cursor, int limit);
		Task MarkAsReadAsync(Guid notificationId, Guid userId);
	}
}
