using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IFeedService
	{
		Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request);
		Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit);
		Task FanoutPostToFeedAsync(Guid postId, Guid authorId);

		Task BackfillFolloweeFeedAsync(Guid followerId, Guid followeeId);

		Task InvalidateFeedCacheAsync(Guid userId);

		Task<int> GetNewPostsCountAsync(Guid userId, DateTime since);
	}
}