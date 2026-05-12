using System.Text.Json;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Persistence.Services
{
	public class OutboxJobPublisher : IBackgroundJobPublisher
	{
		private readonly AppDbContext _context;

		public OutboxJobPublisher(AppDbContext context)
		{
			_context = context;
		}

		public void EnqueuePostFanoutJob(Guid postId, Guid authorId)
		{
			var payload = JsonSerializer.Serialize(new { PostId = postId, AuthorId = authorId });
			AddOutboxMessage("FanoutPostToFeed", payload);
		}

		public void EnqueueCommentNotificationJob(Guid postAuthorId, Guid commentAuthorId, Guid postId, Guid commentId)
		{
			var payload = JsonSerializer.Serialize(new { PostAuthorId = postAuthorId, CommentAuthorId = commentAuthorId, PostId = postId, CommentId = commentId });
			AddOutboxMessage("CreateCommentNotification", payload);
		}

		public void EnqueueLikeNotificationJob(Guid recipientId, Guid actorId, Guid postId)
		{
			var payload = JsonSerializer.Serialize(new { RecipientId = recipientId, ActorId = actorId, PostId = postId });
			AddOutboxMessage("CreateLikeNotification", payload);
		}

		public void EnqueueFollowNotificationJob(Guid recipientId, Guid actorId)
		{
			var payload = JsonSerializer.Serialize(new { RecipientId = recipientId, ActorId = actorId });
			AddOutboxMessage("CreateFollowNotification", payload);
		}

		public void EnqueueBackfillFeedJob(Guid followerId, Guid followeeId)
		{
			var payload = JsonSerializer.Serialize(new { FollowerId = followerId, FolloweeId = followeeId });
			AddOutboxMessage("BackfillFolloweeFeed", payload);
		}

		public void EnqueueInvalidateFeedCacheJob(Guid userId)
		{
			var payload = JsonSerializer.Serialize(new { UserId = userId });
			AddOutboxMessage("InvalidateFeedCache", payload);
		}

		public void EnqueueReportNotificationJob(Guid targetId, Guid reporterId)
		{
			var payload = JsonSerializer.Serialize(new { TargetId = targetId, ReporterId = reporterId });
			AddOutboxMessage("CreateReportNotification", payload);
		}

		private void AddOutboxMessage(string type, string content)
		{
			var message = new OutboxMessage
			{
				Id = Guid.NewGuid(),
				Type = type,
				Content = content,
				OccurredOnUtc = DateTime.UtcNow
			};

			// We do NOT call SaveChangesAsync here! 
			// We just add it to the tracking context. It will be saved automatically 
			// when the Service layer calls SaveChangesAsync on the main entity.
			_context.OutboxMessages.Add(message);
		}
	}
}