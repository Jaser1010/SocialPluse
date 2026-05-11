using Hangfire;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Persistence.Services
{
	public class HangfireJobPublisher : IBackgroundJobPublisher
	{
		private readonly IBackgroundJobClient _backgroundJobClient;

		public HangfireJobPublisher(IBackgroundJobClient backgroundJobClient)
		{
			_backgroundJobClient = backgroundJobClient;
		}

		public void EnqueuePostFanoutJob(Guid postId, Guid authorId)
		{
			_backgroundJobClient.Enqueue<IFeedService>(s => s.FanoutPostToFeedAsync(postId, authorId));
		}

		public void EnqueueCommentNotificationJob(Guid postAuthorId, Guid commentAuthorId, Guid postId, Guid commentId)
		{
			_backgroundJobClient.Enqueue<INotificationService>(s =>
				s.CreateCommentNotificationAsync(postAuthorId, commentAuthorId, postId, commentId));
		}


		public void EnqueueFollowNotificationJob(Guid recipientId, Guid actorId)
		{
			_backgroundJobClient.Enqueue<INotificationService>(s =>
				s.CreateFollowNotificationAsync(recipientId, actorId));
		}

		public void EnqueueBackfillFeedJob(Guid followerId, Guid followeeId)
		{
			_backgroundJobClient.Enqueue<IFeedService>(s =>
				s.BackfillFolloweeFeedAsync(followerId, followeeId));
		}

		public void EnqueueInvalidateFeedCacheJob(Guid userId)
		{
			_backgroundJobClient.Enqueue<IFeedService>(s =>
				s.InvalidateFeedCacheAsync(userId));
		}


		public void EnqueueLikeNotificationJob(Guid recipientId, Guid actorId, Guid postId)
		{
			_backgroundJobClient.Enqueue<INotificationService>(s =>
				s.CreateLikeNotificationAsync(recipientId, actorId, postId));
		}

		public void EnqueueReportNotificationJob(Guid recipientId, Guid actorId)
		{
			_backgroundJobClient.Enqueue<INotificationService>(s =>
				s.CreateReportNotificationAsync(recipientId, actorId));
		}







	}
}