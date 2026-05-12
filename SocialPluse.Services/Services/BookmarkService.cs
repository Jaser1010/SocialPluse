using System.Globalization;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Services
{
	public class BookmarkService : IBookmarkService
	{
		private readonly IPostRepository _postRepository;
		private readonly IPostEnricher _postEnricher;

		public BookmarkService(IPostRepository postRepository, IPostEnricher postEnricher)
		{
			_postRepository = postRepository;
			_postEnricher = postEnricher;
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

			var postDtos = await _postEnricher.EnrichAndOrderPostsAsync(posts, postIds, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = bookmarkRows.Count == clampedLimit
					? ((DateTimeOffset)bookmarkRows.Last().CreatedAt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
					: null,
			};
		}
	}
}