namespace SocialPluse.Domain.Constants
{
	public static class OutboxMessageTypes
	{
		public const string FanoutPostToFeed = nameof(FanoutPostToFeed);
		public const string CreateCommentNotification = nameof(CreateCommentNotification);
		public const string CreateLikeNotification = nameof(CreateLikeNotification);
		public const string CreateFollowNotification = nameof(CreateFollowNotification);
		public const string BackfillFolloweeFeed = nameof(BackfillFolloweeFeed);
		public const string InvalidateFeedCache = nameof(InvalidateFeedCache);
		public const string CreateReportNotification = nameof(CreateReportNotification);
	}
}