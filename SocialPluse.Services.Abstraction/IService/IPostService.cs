using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IPostService
	{
		Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request);
		Task<PostDto> GetByIdAsync(Guid postId, Guid? currentUserId = null);
		Task DeletePostAsync(Guid postId, Guid requestingUserId);
		Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request);

		Task FanoutPostToFeedAsync(Guid postId, Guid authorId);
		Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit);

		/// <summary>Adds all existing posts from a newly followed user into the follower's Redis feed cache.</summary>
		Task BackfillFolloweeFeedAsync(Guid followerId, Guid followeeId);

		/// <summary>Deletes the Redis feed cache for a user so the next load re-queries the DB with current follows.</summary>
		Task InvalidateFeedCacheAsync(Guid userId);

		/// <summary>Returns the count of posts newer than <paramref name="since"/> that belong to the user's feed.</summary>
		Task<int> GetNewPostsCountAsync(Guid userId, DateTime since);

		Task<List<TrendingTopicDto>> GetTrendingTopicsAsync(Guid userId, int limit = 8, int hours = 72);
		Task<bool> ToggleBookmarkAsync(Guid userId, Guid postId, bool shouldBookmark);
		Task<FeedResponse> GetBookmarkedPostsAsync(Guid userId, string? cursor, int limit);
		Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId);
	}
}
