using System.Globalization;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Services
{
	public class FeedService : IFeedService
	{
		private readonly IPostRepository _postRepository;
		private readonly IFeedCacheService _feedCache;
		private readonly IPostEnricher _postEnricher; 

		public FeedService(
			IPostRepository postRepository,
			IFeedCacheService feedCache,
			IPostEnricher postEnricher) 
		{
			_postRepository = postRepository;
			_feedCache = feedCache;
			_postEnricher = postEnricher;
		}

		public async Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request)
		{
			var pageSize = Math.Clamp(request.Limit, 1, 50);
			DateTime? cursorDate = null;

			if (request.Cursor != null && double.TryParse(request.Cursor, NumberStyles.Any, CultureInfo.InvariantCulture, out double ms))
			{
				cursorDate = DateTimeOffset.FromUnixTimeMilliseconds((long)ms).UtcDateTime;
			}

			var posts = await _postRepository.GetFeedPostsAsync(userId, cursorDate, pageSize);

			var postDtos = await _postEnricher.EnrichAndOrderPostsAsync(posts, null, userId);
			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = posts.Count == pageSize
							? ((DateTimeOffset)posts.Last().CreatedAt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
							: null
			};
		}

		public async Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			var (postIds, nextCursor) = await _feedCache.GetCachedFeedAsync(userId, cursor, clampedLimit);

			if (postIds.Count == 0)
			{
				return await GetFeedAsync(userId, new FeedRequest { Cursor = cursor, Limit = clampedLimit });
			}

			var posts = await _postRepository.GetPostsByIdsAsync(postIds);

			var postDtos = await _postEnricher.EnrichAndOrderPostsAsync(posts, postIds, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = nextCursor
			};
		}

		public async Task FanoutPostToFeedAsync(Guid postId, Guid authorId)
		{
			var followerIds = await _postRepository.GetFollowerIdsAsync(authorId);
			followerIds.Add(authorId);
			followerIds = followerIds.Distinct().ToList();

			var post = await _postRepository.GetByIdAsync(postId);
			if (post is null) return;

			var score = (double)((DateTimeOffset)post.CreatedAt).ToUnixTimeMilliseconds();
			await _feedCache.AddPostToFeedsAsync(followerIds, postId, score);
		}

		public async Task BackfillFolloweeFeedAsync(Guid followerId, Guid followeeId)
		{
			var posts = await _postRepository.GetRecentPostsByAuthorAsync(followeeId, 500);
			if (posts.Count == 0) return;

			var postsWithScores = posts.Select(p => (p.Id, (double)((DateTimeOffset)p.CreatedAt).ToUnixTimeMilliseconds()));
			await _feedCache.AddPostsToFeedAsync(followerId, postsWithScores);
		}

		public async Task InvalidateFeedCacheAsync(Guid userId) => await _feedCache.InvalidateFeedCacheAsync(userId);

		public async Task<int> GetNewPostsCountAsync(Guid userId, DateTime since) => await _postRepository.GetNewPostsCountAsync(userId, since);
	}
}