using System;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IBackgroundJobPublisher
	{
		// For PostService
		void EnqueuePostFanoutJob(Guid postId, Guid authorId);

		// For CommentService
		void EnqueueCommentNotificationJob(Guid postAuthorId, Guid commentAuthorId, Guid postId, Guid commentId);


		// For FollowService
		void EnqueueFollowNotificationJob(Guid recipientId, Guid actorId);
		void EnqueueBackfillFeedJob(Guid followerId, Guid followeeId);
		void EnqueueInvalidateFeedCacheJob(Guid userId);
		// For LikeService
		void EnqueueLikeNotificationJob(Guid recipientId, Guid actorId, Guid postId);

		void EnqueueReportNotificationJob(Guid recipientId, Guid actorId);



	}
}