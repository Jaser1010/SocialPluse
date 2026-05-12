using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IBookmarkService
	{
		Task<bool> ToggleBookmarkAsync(Guid userId, Guid postId, bool shouldBookmark);
		Task<FeedResponse> GetBookmarkedPostsAsync(Guid userId, string? cursor, int limit);
	}
}