using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SocialPluse.Services.Mappers;

namespace SocialPluse.Services
{
	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IUserRepository _userRepository;
		private readonly IFeedCacheService _feedCache;
		private readonly IBackgroundJobPublisher _jobPublisher;

		public PostService(
			IPostRepository postRepository,
			IUserRepository userRepository,
			IFeedCacheService feedCache,
			IBackgroundJobPublisher jobPublisher)
		{
			_postRepository = postRepository;
			_userRepository = userRepository;
			_feedCache = feedCache;
			_jobPublisher = jobPublisher;
		}

		public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest createPostRequest)
		{
			var username = await _userRepository.GetUsernameAsync(authorId);
			if (username == null) throw new KeyNotFoundException($"User with ID {authorId} not found.");

			var post = new Post
			{
				Id = Guid.NewGuid(),
				AuthorId = authorId,
				Text = createPostRequest.Text,
				MediaUrl = createPostRequest.MediaUrl,
				CreatedAt = DateTime.UtcNow
			};

			await _postRepository.AddAsync(post);
			var affectedRows = await _postRepository.SaveChangesAsync();
			if (affectedRows <= 0) throw new InvalidOperationException("Failed to save post to the database.");

			_jobPublisher.EnqueuePostFanoutJob(post.Id, post.AuthorId);

			return new PostDto
			{
				Id = post.Id,
				AuthorId = post.AuthorId,
				AuthorUsername = username,
				Text = post.Text,
				MediaUrl = post.MediaUrl,
				LikesCount = 0,
				CommentsCount = 0,
				IsLikedByCurrentUser = false,
				IsBookmarkedByCurrentUser = false,
				CreatedAt = post.CreatedAt
			};
		}

		public async Task DeletePostAsync(Guid postId, Guid requestingUserId)
		{
			var post = await _postRepository.GetByIdAsync(postId);
			if (post == null) throw new KeyNotFoundException($"Post with ID {postId} not found.");
			if (post.AuthorId != requestingUserId) throw new UnauthorizedAccessException("You can only delete your own posts.");

			await _postRepository.DeleteAsync(post);
			if (await _postRepository.SaveChangesAsync() <= 0) throw new Exception("Failed to delete post.");
		}

		public async Task<PostDto> GetByIdAsync(Guid postId, Guid? currentUserId = null)
		{
			var post = await _postRepository.GetByIdAsync(postId);
			if (post == null) throw new KeyNotFoundException($"Post with ID {postId} not found.");
			return (await EnrichPostsAsync([post], currentUserId)).Single();
		}

		public async Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request)
		{
			var pageSize = Math.Clamp(request.Limit, 1, 50);
			DateTime? cursorDate = request.Cursor != null && DateTime.TryParse(request.Cursor, out var cd) ? cd : null;

			var posts = await _postRepository.GetFeedPostsAsync(userId, cursorDate, pageSize);
			var postDtos = await EnrichPostsAsync(posts, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = posts.Count == pageSize ? posts.Last().CreatedAt.ToString("O") : null
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

		public async Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);

			var (postIds, nextCursor) = await _feedCache.GetCachedFeedAsync(userId, cursor, clampedLimit);

			if (postIds.Count == 0)
			{
				if (cursor == null) return await GetFeedAsync(userId, new FeedRequest { Cursor = null, Limit = clampedLimit });
				return new FeedResponse { Posts = [], NextCursor = null };
			}

			var posts = await _postRepository.GetPostsByIdsAsync(postIds);

			var postMap = posts.ToDictionary(p => p.Id);
			var orderedPosts = postIds.Where(postMap.ContainsKey).Select(id => postMap[id]).ToList();
			var postDtos = await EnrichPostsAsync(orderedPosts, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = nextCursor
			};
		}

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

		public async Task BackfillFolloweeFeedAsync(Guid followerId, Guid followeeId)
		{
			var posts = await _postRepository.GetRecentPostsByAuthorAsync(followeeId, 500);
			if (posts.Count == 0) return;

			var postsWithScores = posts.Select(p => (p.Id, (double)((DateTimeOffset)p.CreatedAt).ToUnixTimeMilliseconds()));

			await _feedCache.AddPostsToFeedAsync(followerId, postsWithScores);
		}

		public async Task InvalidateFeedCacheAsync(Guid userId)
		{
			await _feedCache.InvalidateFeedCacheAsync(userId);
		}

		public async Task<int> GetNewPostsCountAsync(Guid userId, DateTime since)
		{
			return await _postRepository.GetNewPostsCountAsync(userId, since);
		}

		public async Task<List<TrendingTopicDto>> GetTrendingTopicsAsync(Guid userId, int limit = 8, int hours = 72)
		{
			var clampedLimit = Math.Clamp(limit, 1, 20);
			var from = DateTime.UtcNow.AddHours(-Math.Clamp(hours, 1, 168));

			var postTexts = await _postRepository.GetRecentPostTextsAsync(userId, from, 3000);
			var hashtagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var text in postTexts)
			{
				if (string.IsNullOrWhiteSpace(text)) continue;
				var matches = Regex.Matches(text, @"#([A-Za-z0-9_]{2,50})");
				foreach (Match match in matches)
				{
					var tag = "#" + match.Groups[1].Value;
					hashtagCounts[tag] = hashtagCounts.TryGetValue(tag, out var current) ? current + 1 : 1;
				}
			}

			return hashtagCounts.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key).Take(clampedLimit)
				.Select(kvp => new TrendingTopicDto { Hashtag = kvp.Key, Mentions = kvp.Value }).ToList();
		}

		public async Task<bool> ToggleBookmarkAsync(Guid userId, Guid postId, bool shouldBookmark)
		{
			var bookmarkExists = await _postRepository.BookmarkExistsAsync(userId, postId);
			if (shouldBookmark)
			{
				if (bookmarkExists) return true;
				if (!await _postRepository.PostExistsAsync(postId)) throw new KeyNotFoundException("Post not found.");

				await _postRepository.AddBookmarkAsync(userId, postId);
				await _postRepository.SaveChangesAsync();
				return true;
			}

			if (!bookmarkExists) return false;
			await _postRepository.RemoveBookmarkAsync(userId, postId);
			await _postRepository.SaveChangesAsync();
			return false;
		}

		public async Task<FeedResponse> GetBookmarkedPostsAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			DateTime? cursorDate = !string.IsNullOrWhiteSpace(cursor) && DateTime.TryParse(cursor, out var cd) ? cd : null;

			var bookmarkRows = await _postRepository.GetBookmarksAsync(userId, cursorDate, clampedLimit);
			var postIds = bookmarkRows.Select(b => b.PostId).ToList();
			var posts = await _postRepository.GetPostsByIdsAsync(postIds);

			var postMap = posts.ToDictionary(p => p.Id);
			var orderedPosts = postIds.Where(postMap.ContainsKey).Select(id => postMap[id]).ToList();
			var postDtos = await EnrichPostsAsync(orderedPosts, userId);

			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = bookmarkRows.Count == clampedLimit ? bookmarkRows.Last().CreatedAt.ToString("O") : null,
			};
		}

		public async Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId)
		{
			return await _postRepository.GetUserAnalyticsAsync(userId);
		}
	}
}