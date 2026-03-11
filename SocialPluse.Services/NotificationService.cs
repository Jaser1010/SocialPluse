	using Microsoft.AspNetCore.Identity;
	using Microsoft.EntityFrameworkCore;
	using SocialPluse.Domain.Entities;
	using SocialPluse.Domain.Enums;
	using SocialPluse.Persistence.DbContexts;
	using SocialPluse.Persistence.IdentityData.Entities;
	using SocialPluse.Services.Abstraction;
	using SocialPluse.Shared.DTOs.Notifications;

	namespace SocialPluse.Services
	{
		public class NotificationService : INotificationService
		{
			private readonly AppDbContext _appDbContext;
			private readonly UserManager<AppUser> _userManager;
			private readonly INotificationSender _notificationSender;

			public NotificationService(AppDbContext appDbContext, UserManager<AppUser> userManager, INotificationSender notificationSender)
			{
				_appDbContext = appDbContext;
				_userManager = userManager;
				_notificationSender = notificationSender;
			}



			public async Task CreateCommentNotificationAsync(Guid recipientId, Guid actorId, Guid postId, Guid commentId)
			{
				var actor = await _userManager.FindByIdAsync(actorId.ToString());

				var notification = new Notification
				{
					Id = Guid.NewGuid(),
					RecipientUserId = recipientId,
					ActorUserId = actorId,
					Type = NotificationType.Comment,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_appDbContext.Notifications.Add(notification);
				await _appDbContext.SaveChangesAsync();

				await _notificationSender.SendAsync(recipientId, new NotificationDto
				{
					Id = notification.Id,
					ActorUserId = actorId,
					ActorUsername = actor?.UserName ?? "Unknown",
					Type = NotificationType.Comment,
					PostId = postId,
					CommentId = commentId,
					IsRead = false,
					CreatedAt = notification.CreatedAt
				});
			}

			public async Task CreateFollowNotificationAsync(Guid recipientId, Guid actorId)
			{
				var actor = await _userManager.FindByIdAsync(actorId.ToString());

				var notification = new Notification
				{
					Id = Guid.NewGuid(),
					RecipientUserId = recipientId,
					ActorUserId = actorId,
					Type = NotificationType.Follow,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_appDbContext.Notifications.Add(notification);
				await _appDbContext.SaveChangesAsync();

				await _notificationSender.SendAsync(recipientId, new NotificationDto
				{
					Id = notification.Id,
					ActorUserId = actorId,
					ActorUsername = actor?.UserName ?? "Unknown",
					Type = NotificationType.Follow,
					IsRead = false,
					CreatedAt = notification.CreatedAt
				});
			}

			public async Task CreateLikeNotificationAsync(Guid recipientId, Guid actorId, Guid postId)
			{
				var actor = await _userManager.FindByIdAsync(actorId.ToString());

				var notification = new Notification
				{
					Id = Guid.NewGuid(),
					RecipientUserId = recipientId,
					ActorUserId = actorId,
					Type = NotificationType.Like,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_appDbContext.Notifications.Add(notification);
				await _appDbContext.SaveChangesAsync();

				await _notificationSender.SendAsync(recipientId, new NotificationDto
				{
					Id = notification.Id,
					ActorUserId = actorId,
					ActorUsername = actor?.UserName ?? "Unknown",
					Type = NotificationType.Like,
					PostId = postId,
					IsRead = false,
					CreatedAt = notification.CreatedAt
				});
			}

			public async Task<NotificationResponse> GetNotificationsAsync(Guid userId, DateTime? cursor, int limit)
			{
				var query = _appDbContext.Notifications.Where(n => n.RecipientUserId == userId);

				if (cursor.HasValue)
					query = query.Where(n => n.CreatedAt < cursor.Value);

				var clampedLimit = Math.Clamp(limit, 1, 50);

				var notifications = await query
					.OrderByDescending(n => n.CreatedAt)
					.Take(clampedLimit).ToListAsync<Notification>();

				// Batch fetch actor usernames — same pattern as feed
				var actorIds = notifications.Select(n => n.ActorUserId).Distinct().ToList();
				var actors = await _userManager.Users
					.Where(u => actorIds.Contains(u.Id))
					.ToDictionaryAsync(u => u.Id, u => u.UserName!);

				return new NotificationResponse
				{
					Notifications = notifications.Select(n => new NotificationDto
					{
						Id = n.Id,
						ActorUserId = n.ActorUserId,
						ActorUsername = actors.GetValueOrDefault(n.ActorUserId, "Unknown"),
						Type = n.Type,
						PostId = n.PostId,
						CommentId = n.CommentId,
						IsRead = n.IsRead,
						CreatedAt = n.CreatedAt
					}).ToList(),

					NextCursor = notifications.Count == clampedLimit? notifications.Last().CreatedAt: null
				};
			}

			public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
			{
				// 1. FindAsync(notificationId) → KeyNotFoundException if null
				var notification = await _appDbContext.Notifications.FindAsync(notificationId);
				if (notification == null)	throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
				// 2. Check RecipientUserId == userId → UnauthorizedAccessException if not
				var isRecipient = notification.RecipientUserId == userId;
				if (!isRecipient) throw new UnauthorizedAccessException("You are not authorized to mark this notification as read.");
				// 3. Set IsRead = true
				notification.IsRead = true;
				// 4. SaveChangesAsync
				await _appDbContext.SaveChangesAsync();
			}
		}
	}
