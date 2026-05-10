using SocialPluse.Services.Abstraction.IService;
using StackExchange.Redis;
using System.Globalization;


namespace SocialPluse.Persistence.Services
{
	public class RedisFeedCacheService : IFeedCacheService
	{
		private readonly IConnectionMultiplexer _redis;

		public RedisFeedCacheService(IConnectionMultiplexer redis)
		{
			_redis = redis;
		}

		public async Task AddPostToFeedsAsync(IEnumerable<Guid> followerIds, Guid postId, double score)
		{
			var db = _redis.GetDatabase();
			var batch = db.CreateBatch(); // Optimization: Execute all Redis commands in one batch
			var tasks = new List<Task>();

			foreach (var followerId in followerIds)
			{
				var feedKey = $"feed:{followerId}";
				tasks.Add(batch.SortedSetAddAsync(feedKey, postId.ToString(), score));
				tasks.Add(batch.SortedSetRemoveRangeByRankAsync(feedKey, 0, -501)); // keep max 500
				tasks.Add(batch.KeyExpireAsync(feedKey, TimeSpan.FromDays(7)));
			}

			batch.Execute();
			await Task.WhenAll(tasks);
		}

		public async Task AddPostsToFeedAsync(Guid userId, IEnumerable<(Guid PostId, double Score)> posts)
		{
			var db = _redis.GetDatabase();
			var key = $"feed:{userId}";

			var entries = posts.Select(p => new SortedSetEntry(p.PostId.ToString(), p.Score)).ToArray();

			await db.SortedSetAddAsync(key, entries);
			await db.SortedSetRemoveRangeByRankAsync(key, 0, -501);
			await db.KeyExpireAsync(key, TimeSpan.FromDays(7));
		}

		public async Task<(List<Guid> PostIds, string? NextCursor)> GetCachedFeedAsync(Guid userId, string? cursor, int limit)
		{
			var db = _redis.GetDatabase();
			var key = $"feed:{userId}";
			double maxScore = cursor != null
					? double.Parse(cursor, CultureInfo.InvariantCulture) - 1
					: double.MaxValue;

			var feedEntries = await db.SortedSetRangeByScoreWithScoresAsync(
				key,
				start: double.NegativeInfinity,
				stop: maxScore,
				order: Order.Descending,
				take: limit);

			if (feedEntries.Length == 0)
			{
				return (new List<Guid>(), null);
			}

			var postIds = feedEntries.Select(e => Guid.Parse((string)e.Element!)).ToList();
			string? nextCursor = feedEntries.Length == limit
						? feedEntries.Last().Score.ToString("R", CultureInfo.InvariantCulture)
						: null;

			return (postIds, nextCursor);
		}

		public async Task InvalidateFeedCacheAsync(Guid userId)
		{
			var db = _redis.GetDatabase();
			await db.KeyDeleteAsync($"feed:{userId}");
		}
	}
}