using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Persistence.Repositories
{
	public class PostRepository : IPostRepository
	{
		private readonly AppDbContext _context;

		public PostRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<Post?> GetByIdAsync(Guid id) => await _context.Posts.FindAsync(id);
		public async Task AddAsync(Post post) => await _context.Posts.AddAsync(post);
		public async Task DeleteAsync(Post post) => _context.Posts.Remove(post);
		public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
		public async Task<bool> PostExistsAsync(Guid postId) => await _context.Posts.AnyAsync(p => p.Id == postId);

		public async Task<List<Post>> GetFeedPostsAsync(Guid userId, DateTime? cursor, int limit)
		{
			var followeeIds = await _context.Follows
				.Where(f => f.FollowerId == userId)
				.Select(f => f.FolloweeId)
				.ToListAsync();

			followeeIds.Add(userId);
			var query = _context.Posts.AsNoTracking().Where(p => followeeIds.Contains(p.AuthorId));

			if (cursor.HasValue)
				query = query.Where(p => p.CreatedAt < cursor.Value);

			return await query.OrderByDescending(p => p.CreatedAt).Take(limit).ToListAsync();
		}

		public async Task<List<Post>> GetPostsByIdsAsync(IEnumerable<Guid> postIds)
		{
			return await _context.Posts.AsNoTracking().Where(p => postIds.Contains(p.Id)).ToListAsync();
		}

		public async Task<List<Post>> GetRecentPostsByAuthorAsync(Guid authorId, int limit)
		{
			return await _context.Posts
				.AsNoTracking()
				.Where(p => p.AuthorId == authorId)
				.OrderByDescending(p => p.CreatedAt)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<int> GetNewPostsCountAsync(Guid userId, DateTime since)
		{
			var followeeIds = await _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId).ToListAsync();
			followeeIds.Add(userId);
			return await _context.Posts.CountAsync(p => followeeIds.Contains(p.AuthorId) && p.CreatedAt > since);
		}

		public async Task<List<string>> GetRecentPostTextsAsync(Guid userId, DateTime since, int limit)
		{
			var followeeIds = await _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId).ToListAsync();
			followeeIds.Add(userId);
			return await _context.Posts
				.Where(p => followeeIds.Contains(p.AuthorId) && p.CreatedAt >= since)
				.OrderByDescending(p => p.CreatedAt)
				.Select(p => p.Text)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<List<Guid>> GetFollowerIdsAsync(Guid userId)
		{
			return await _context.Follows.Where(f => f.FolloweeId == userId).Select(f => f.FollowerId).ToListAsync();
		}

		public async Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> postIds)
		{
			return await _context.Likes.Where(l => postIds.Contains(l.PostId))
				.GroupBy(l => l.PostId)
				.ToDictionaryAsync(g => g.Key, g => g.Count());
		}

		public async Task<Dictionary<Guid, int>> GetCommentCountsAsync(IEnumerable<Guid> postIds)
		{
			return await _context.Comments.Where(c => postIds.Contains(c.PostId))
				.GroupBy(c => c.PostId)
				.ToDictionaryAsync(g => g.Key, g => g.Count());
		}

		public async Task<HashSet<Guid>> GetLikedPostIdsAsync(Guid userId, IEnumerable<Guid> postIds)
		{
			var list = await _context.Likes.Where(l => l.UserId == userId && postIds.Contains(l.PostId)).Select(l => l.PostId).ToListAsync();
			return list.ToHashSet();
		}

		public async Task<HashSet<Guid>> GetBookmarkedPostIdsAsync(Guid userId, IEnumerable<Guid> postIds)
		{
			var list = await _context.Bookmarks.Where(b => b.UserId == userId && postIds.Contains(b.PostId)).Select(b => b.PostId).ToListAsync();
			return list.ToHashSet();
		}

		public async Task<bool> BookmarkExistsAsync(Guid userId, Guid postId)
		{
			return await _context.Bookmarks.AnyAsync(b => b.UserId == userId && b.PostId == postId);
		}

		public async Task AddBookmarkAsync(Guid userId, Guid postId)
		{
			_context.Bookmarks.Add(new Bookmark { UserId = userId, PostId = postId, CreatedAt = DateTime.UtcNow });
		}

		public async Task RemoveBookmarkAsync(Guid userId, Guid postId)
		{
			var bookmark = await _context.Bookmarks.FindAsync(userId, postId);
			if (bookmark != null) _context.Bookmarks.Remove(bookmark);
		}

		public async Task<List<Bookmark>> GetBookmarksAsync(Guid userId, DateTime? cursor, int limit)
		{
			var query = _context.Bookmarks.AsNoTracking().Where(b => b.UserId == userId);
			if (cursor.HasValue) query = query.Where(b => b.CreatedAt < cursor.Value);
			return await query.OrderByDescending(b => b.CreatedAt).Take(limit).ToListAsync();
		}

		public async Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId)
		{
			var postsCount = await _context.Posts.CountAsync(p => p.AuthorId == userId);
			var followersCount = await _context.Follows.CountAsync(f => f.FolloweeId == userId);
			var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == userId);
			var bookmarksCount = await _context.Bookmarks.CountAsync(b => b.UserId == userId);
			var unreadNotifications = await _context.Notifications.CountAsync(n => n.RecipientUserId == userId && !n.IsRead);

			var myPostIds = await _context.Posts.AsNoTracking().Where(p => p.AuthorId == userId).Select(p => p.Id).ToListAsync();

			var likesReceived = myPostIds.Count > 0 ? await _context.Likes.CountAsync(l => myPostIds.Contains(l.PostId)) : 0;
			var commentsReceived = myPostIds.Count > 0 ? await _context.Comments.CountAsync(c => myPostIds.Contains(c.PostId)) : 0;

			return new UserAnalyticsDto
			{
				PostsCount = postsCount,
				FollowersCount = followersCount,
				FollowingCount = followingCount,
				BookmarksCount = bookmarksCount,
				UnreadNotifications = unreadNotifications,
				LikesReceived = likesReceived,
				CommentsReceived = commentsReceived
			};
		}


		public async Task<IDbContextTransaction> BeginTransactionAsync() =>
				await _context.Database.BeginTransactionAsync();
	}
}