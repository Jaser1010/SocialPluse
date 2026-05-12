using System.Globalization;
using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Services
{
	public class BookmarkService : IBookmarkService
	{
		private readonly IPostRepository _postRepository;
		private readonly IUserRepository _userRepository;

		public BookmarkService(IPostRepository postRepository, IUserRepository userRepository)
		{
			_postRepository = postRepository;
			_userRepository = userRepository;
		}

		public async Task<bool> ToggleBookmarkAsync(Guid userId, Guid postId, bool shouldBookmark)
		{
			using var transaction = await _postRepository.BeginTransactionAsync();
			try
			{
				var bookmarkExists = await _postRepository.BookmarkExistsAsync(userId, postId);

				if (shouldBookmark)
				{
					if (bookmarkExists) { await transaction.CommitAsync(); return true; }

					if (!await _postRepository.PostExistsAsync(postId))
						throw new KeyNotFoundException($"Post with ID {postId} not found.");

					await _postRepository.AddBookmarkAsync(userId, postId);
					await _postRepository.SaveChangesAsync();

					await transaction.CommitAsync();
					return true;
				}

				if (!bookmarkExists) { await transaction.CommitAsync(); return false; }

				await _postRepository.RemoveBookmarkAsync(userId, postId);
				await _postRepository.SaveChangesAsync();

				await transaction.CommitAsync();
				return false;
			}
			catch { await transaction.RollbackAsync(); throw; }
		}

		public async Task<FeedResponse> GetBookmarkedPostsAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			DateTime? cursorDate = null;

			if (cursor != null && double.TryParse(cursor, NumberStyles.Any, CultureInfo.InvariantCulture, out double ms))
			{
				cursorDate = DateTimeOffset.FromUnixTimeMilliseconds((long)ms).UtcDateTime;
			}

			var bookmarkRows = await _postRepository.GetBookmarksAsync(userId, cursorDate, clampedLimit);
			var postIds = bookmarkRows.Select(b => b.PostId).ToList();
			var posts = await _postRepository.GetPostsByIdsAsync(postIds);

			var postMap = posts.ToDictionary(p => p.Id);
			var orderedPosts = postIds.Where(postMap.ContainsKey).Select(id => postMap[id]).ToList();
			var postDtos = await EnrichPostsAsync(orderedPosts, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = bookmarkRows.Count == clampedLimit
					? ((DateTimeOffset)bookmarkRows.Last().CreatedAt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
					: null,
			};
		}

		// Private helper to avoid circular dependencies while remaining decoupled
		private async Task<List<PostDto>> EnrichPostsAsync(List<Post> posts, Guid? currentUserId = null)
		{
			if (posts.Count == 0) return [];
			var postIds = posts.Select(p => p.Id).ToList();
			var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
			var authorUsernames = await _userRepository.GetUsernamesAsync(authorIds);
			var likeCounts = await _postRepository.GetLikeCountsAsync(postIds);
			var commentCounts = await _postRepository.GetCommentCountsAsync(postIds);

			HashSet<Guid> likedPostIds = [];
			HashSet<Guid> bookmarkedPostIds = [];
			if (currentUserId.HasValue)
			{
				likedPostIds = await _postRepository.GetLikedPostIdsAsync(currentUserId.Value, postIds);
				bookmarkedPostIds = await _postRepository.GetBookmarkedPostIdsAsync(currentUserId.Value, postIds);
			}

			return posts.Select(p => p.ToDto(
				authorUsernames.GetValueOrDefault(p.AuthorId, "Unknown"),
				likeCounts.GetValueOrDefault(p.Id, 0),
				commentCounts.GetValueOrDefault(p.Id, 0),
				likedPostIds.Contains(p.Id),
				bookmarkedPostIds.Contains(p.Id)
			)).ToList();
		}
	}
}