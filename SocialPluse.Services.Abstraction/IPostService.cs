using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Abstraction
{
	public interface IPostService
	{
		Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request);
		Task<PostDto> GetByIdAsync(Guid postId, Guid? currentUserId = null);
		Task DeletePostAsync(Guid postId, Guid requestingUserId);
		Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request);

		Task FanoutPostToFeedAsync(Guid postId, Guid authorId);
		Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit);

	}
}
