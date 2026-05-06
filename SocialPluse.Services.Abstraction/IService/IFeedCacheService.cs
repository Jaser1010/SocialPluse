using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IFeedCacheService
	{
		Task AddPostToFeedsAsync(IEnumerable<Guid> followerIds, Guid postId, double score);
		Task AddPostsToFeedAsync(Guid userId, IEnumerable<(Guid PostId, double Score)> posts);
		Task<(List<Guid> PostIds, string? NextCursor)> GetCachedFeedAsync(Guid userId, string? cursor, int limit);
		Task InvalidateFeedCacheAsync(Guid userId);
	}
}