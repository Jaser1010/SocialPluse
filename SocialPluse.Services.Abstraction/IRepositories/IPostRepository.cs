using Microsoft.EntityFrameworkCore.Storage;
using SocialPluse.Domain.Entities;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface IPostRepository
	{
		// Basic CRUD
		Task<Post?> GetByIdAsync(Guid id);
		Task AddAsync(Post post);
		Task DeleteAsync(Post post);
		Task<int> SaveChangesAsync();
		Task<bool> PostExistsAsync(Guid postId);

		// Feeds & Posts
		Task<List<Post>> GetFeedPostsAsync(Guid userId, DateTime? cursor, int limit);
		Task<List<Post>> GetPostsByIdsAsync(IEnumerable<Guid> postIds);
		Task<List<Post>> GetRecentPostsByAuthorAsync(Guid authorId, int limit);
		Task<int> GetNewPostsCountAsync(Guid userId, DateTime since);
		Task<List<string>> GetRecentPostTextsAsync(Guid userId, DateTime since, int limit);

		// Follows
		Task<List<Guid>> GetFollowerIdsAsync(Guid userId);

		// Engagement (Likes & Comments)
		Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> postIds);
		Task<Dictionary<Guid, int>> GetCommentCountsAsync(IEnumerable<Guid> postIds);
		Task<HashSet<Guid>> GetLikedPostIdsAsync(Guid userId, IEnumerable<Guid> postIds);

		// Bookmarks
		Task<HashSet<Guid>> GetBookmarkedPostIdsAsync(Guid userId, IEnumerable<Guid> postIds);
		Task<bool> BookmarkExistsAsync(Guid userId, Guid postId);
		Task AddBookmarkAsync(Guid userId, Guid postId);
		Task RemoveBookmarkAsync(Guid userId, Guid postId);
		Task<List<Bookmark>> GetBookmarksAsync(Guid userId, DateTime? cursor, int limit);

		// Analytics
		Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId);

		Task<IDbContextTransaction> BeginTransactionAsync();

		Task<int> GetPostCountAsync(Guid userId);
	}
}
