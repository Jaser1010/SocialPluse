using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Posts;
using StackExchange.Redis;
using Hangfire;

namespace SocialPluse.Services
{
	public class PostService : IPostService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly AppDbContext _appDbContext;
		private readonly IConnectionMultiplexer _redis;

		public PostService(UserManager<AppUser> userManager, AppDbContext appDbContext,IConnectionMultiplexer redis)
		{
			_userManager = userManager;
			_appDbContext = appDbContext;
			_redis = redis;
		}


		public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request)
		{
			// 1. Find user by authorId using UserManager → if null throw KeyNotFoundException
			var user = await _userManager.FindByIdAsync(authorId.ToString());
			if (user == null)	throw new KeyNotFoundException($"User with ID {authorId} not found.");
			// 2. Create new Post entity and save to database
			var post = new Post
			{
				Id = Guid.NewGuid(),
				AuthorId = authorId,
				Text = request.Text,
				MediaUrl = request.MediaUrl,
				CreatedAt = DateTime.UtcNow
			};
			// 3. Add(post)
			var entry = _appDbContext.Posts.Add(post);
			// 4. SaveChangesAsync()
			var result = await _appDbContext.SaveChangesAsync();
			// 5. Fanout to followers' feeds using Hangfire background job
			BackgroundJob.Enqueue<IPostService>(s =>
					s.FanoutPostToFeedAsync(post.Id, post.AuthorId));
			if (result <= 0) throw new Exception("Failed to create post.");
			 var createdPost = entry.Entity;
			// 6. Return PostDto — map the fields
			return new PostDto
			{
				Id = createdPost.Id,
				AuthorId = createdPost.AuthorId,
				AuthorUsername = user.UserName!,
				Text = createdPost.Text,
				MediaUrl = createdPost.MediaUrl,
				CreatedAt = createdPost.CreatedAt
			};
		}

		public async Task DeletePostAsync(Guid postId, Guid requestingUserId)
		{
			// 1. Find post
			var post = await _appDbContext.Posts.FindAsync(postId);
			if (post == null) throw new KeyNotFoundException($"Post with ID {postId} not found.");
			if (post.AuthorId != requestingUserId) throw new UnauthorizedAccessException("You can only delete your own posts.");
			// 3. Remove(post)
			var entry = _appDbContext.Posts.Remove(post);
			// 4. SaveChangesAsync()
			var result = await _appDbContext.SaveChangesAsync();
			if (result <= 0) throw new Exception("Failed to delete post.");
			return;
		}

		public async Task<PostDto> GetByIdAsync(Guid postId)
		{
			// 1. Query: FindAsync(postId)
			var post = await _appDbContext.Posts.FindAsync(postId);
			if (post == null) throw new KeyNotFoundException($"Post with ID {postId} not found.");
			// 2. Get author username
			var author = await _userManager.FindByIdAsync(post.AuthorId.ToString());
			// 3. Return PostDto
			return new PostDto
			{
				Id = post.Id,
				AuthorId = post.AuthorId,
				AuthorUsername = author?.UserName ?? "Unknown",
				Text = post.Text,
				MediaUrl = post.MediaUrl,
				CreatedAt = post.CreatedAt
			};
		}

		public async Task<FeedResponse> GetFeedAsync(Guid userId, FeedRequest request)
		{
			// 1. Get list of followee IDs the user follows
			var followeeIds = await _appDbContext.Follows
															.Where(f => f.FollowerId == userId)
															.Select(f => f.FolloweeId)
															.ToListAsync();
			// 2. Query posts from followees with cursor pagination
			var query = _appDbContext.Posts.Where(p => followeeIds.Contains(p.AuthorId));

			if (request.Cursor != null && DateTime.TryParse(request.Cursor, out var cursorDate))
				query = query.Where(p => p.CreatedAt < cursorDate);

			var limit = Math.Clamp(request.Limit, 1, 50);

			var posts = await query
				.OrderByDescending(p => p.CreatedAt)
				.Take(limit)
				.ToListAsync();
			// 3. Build PostDto list — you'll need usernames, so fetch them
			var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
			var authors = await _userManager.Users.Where(u => authorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName!);
			var postDtos = posts.Select(p => new PostDto
			{
				Id = p.Id,
				AuthorId = p.AuthorId,
				AuthorUsername = authors.ContainsKey(p.AuthorId) ? authors[p.AuthorId] : "Unknown",
				Text = p.Text,
				MediaUrl = p.MediaUrl,
				CreatedAt = p.CreatedAt
			}).ToList();
			// 4. Set NextCursor and return
			return new FeedResponse
			{
				Posts = postDtos,
				NextCursor = posts.Count == limit ? posts.Last().CreatedAt.ToString("O") : null
			};

		}




		public async Task FanoutPostToFeedAsync(Guid postId, Guid authorId)
		{
			// 1. Get all follower IDs
			var followerIds = await _appDbContext.Follows
				.Where(f => f.FolloweeId == authorId)
				.Select(f => f.FollowerId)
				.ToListAsync();

			// 2. Get post for timestamp score
			var post = await _appDbContext.Posts.FindAsync(postId);
			if (post is null) return;
			var score = (double)((DateTimeOffset)post.CreatedAt).ToUnixTimeMilliseconds();

			// 3. Push to each follower's Redis sorted set
			var db = _redis.GetDatabase();
			foreach (var followerId in followerIds)
			{
				var key = $"feed:{followerId}";
				await db.SortedSetAddAsync(key, postId.ToString(), score);
				await db.SortedSetRemoveRangeByRankAsync(key, 0, -501); // keep max 500
				await db.KeyExpireAsync(key, TimeSpan.FromDays(7));
			}
		}
		public async Task<FeedResponse> GetFeedFromCacheAsync(Guid userId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			var db = _redis.GetDatabase();
			var key = $"feed:{userId}";

			double maxScore = cursor != null ? double.Parse(cursor) - 1 : double.MaxValue;

			var entries = await db.SortedSetRangeByScoreWithScoresAsync(
				key,
				start: double.NegativeInfinity,
				stop: maxScore,
				order: Order.Descending,
				take: clampedLimit);

			if (entries.Length == 0)
				return new FeedResponse { Posts = [], NextCursor = null };

			// Batch fetch posts preserving Redis order
			var postIds = entries.Select(e => Guid.Parse((string)e.Element!)).ToList();
			var posts = await _appDbContext.Posts
				.Where(p => postIds.Contains(p.Id))
				.ToListAsync();

			var postMap = posts.ToDictionary(p => p.Id);
			var orderedPosts = postIds
				.Where(id => postMap.ContainsKey(id))
				.Select(id => postMap[id])
				.ToList();

			// Batch fetch usernames
			var authorIds = orderedPosts.Select(p => p.AuthorId).Distinct().ToList();
			var authors = await _userManager.Users
				.Where(u => authorIds.Contains(u.Id))
				.ToDictionaryAsync(u => u.Id, u => u.UserName!);

			return new FeedResponse
			{
				Posts = orderedPosts.Select(p => new PostDto
				{
					Id = p.Id,
					AuthorId = p.AuthorId,
					AuthorUsername = authors.GetValueOrDefault(p.AuthorId, "Unknown"),
					Text = p.Text,
					MediaUrl = p.MediaUrl,
					CreatedAt = p.CreatedAt
				}).ToList(),
				NextCursor = entries.Length == clampedLimit
					? entries.Last().Score.ToString()
					: null
			};
		}
	}
}
