using SocialPluse.Domain.Entities;
using SocialPluse.Domain.Enums;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Notifications;
using System.Globalization;

namespace SocialPluse.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IUserRepository _userRepository;
		private readonly INotificationSender _notificationSender;

		public NotificationService(
			INotificationRepository notificationRepository,
			IUserRepository userRepository,
			INotificationSender notificationSender)
		{
			_notificationRepository = notificationRepository;
			_userRepository = userRepository;
			_notificationSender = notificationSender;
		}

		public async Task CreateCommentNotificationAsync(Guid recipientId, Guid actorId, Guid postId, Guid commentId)
		{
			var actorUsername = await _userRepository.GetUsernameAsync(actorId);

			var notification = new Notification
			{
				Id = Guid.NewGuid(),
				RecipientUserId = recipientId,
				ActorUserId = actorId,
				Type = NotificationType.Comment,
				PostId = postId,
				CommentId = commentId,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			await _notificationRepository.AddAsync(notification);
			await _notificationRepository.SaveChangesAsync();

			// Senior 1-Liner:
			await _notificationSender.SendAsync(recipientId, notification.ToDto(actorUsername ?? "Unknown"));
		}

		public async Task CreateFollowNotificationAsync(Guid recipientId, Guid actorId)
		{
			var actorUsername = await _userRepository.GetUsernameAsync(actorId);

			var notification = new Notification
			{
				Id = Guid.NewGuid(),
				RecipientUserId = recipientId,
				ActorUserId = actorId,
				Type = NotificationType.Follow,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			await _notificationRepository.AddAsync(notification);
			await _notificationRepository.SaveChangesAsync();

			// Senior 1-Liner:
			await _notificationSender.SendAsync(recipientId, notification.ToDto(actorUsername ?? "Unknown"));
		}

		public async Task CreateLikeNotificationAsync(Guid recipientId, Guid actorId, Guid postId)
		{
			var actorUsername = await _userRepository.GetUsernameAsync(actorId);

			var notification = new Notification
			{
				Id = Guid.NewGuid(),
				RecipientUserId = recipientId,
				ActorUserId = actorId,
				Type = NotificationType.Like,
				PostId = postId,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			await _notificationRepository.AddAsync(notification);
			await _notificationRepository.SaveChangesAsync();

			// Senior 1-Liner:
			await _notificationSender.SendAsync(recipientId, notification.ToDto(actorUsername ?? "Unknown"));
		}

		public async Task CreateReportNotificationAsync(Guid recipientId, Guid actorId)
		{
			var notification = new Notification
			{
				Id = Guid.NewGuid(),
				RecipientUserId = recipientId,
				ActorUserId = actorId,
				Type = NotificationType.Report,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			await _notificationRepository.AddAsync(notification);
			await _notificationRepository.SaveChangesAsync();

			// Senior 1-Liner:
			await _notificationSender.SendAsync(recipientId, notification.ToDto("Anonymous"));
		}

		public async Task<NotificationResponse> GetNotificationsAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			DateTime? cursorDate = cursor != null && 
						DateTime.TryParse(cursor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDate)
						? parsedDate
						: null;

			var notifications = await _notificationRepository.GetNotificationsAsync(userId, cursorDate, clampedLimit);

			var actorIds = notifications.Select(n => n.ActorUserId).Distinct().ToList();
			var actors = await _userRepository.GetUsernamesAsync(actorIds);

			return new NotificationResponse
			{
				// Senior 1-Liner Array Mapping:
				Notifications = notifications.Select(n => n.ToDto(
					n.Type == NotificationType.Report ? "Anonymous" : actors.GetValueOrDefault(n.ActorUserId, "Unknown")
				)).ToList(),

				NextCursor = notifications.Count == clampedLimit
							? notifications.Last().CreatedAt.ToString("O", CultureInfo.InvariantCulture)
							: null
			};
		}

		public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
		{
			var notification = await _notificationRepository.GetByIdAsync(notificationId);
			if (notification == null) throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");

			var isRecipient = notification.RecipientUserId == userId;
			if (!isRecipient) throw new UnauthorizedAccessException("You are not authorized to mark this notification as read.");

			notification.IsRead = true;
			await _notificationRepository.SaveChangesAsync();
		}
	}
}