using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Services
{
	public class PostEnricher : IPostEnricher
	{
		private readonly IPostRepository _postRepository;
		private readonly IUserRepository _userRepository;

		public PostEnricher(IPostRepository postRepository, IUserRepository userRepository)
		{
			_postRepository = postRepository;
			_userRepository = userRepository;
		}

		public async Task<List<PostDto>> EnrichAndOrderPostsAsync(List<Post> unorderedPosts, List<Guid>? orderedIds = null, Guid? currentUserId = null)
		{
			if (unorderedPosts.Count == 0) return [];

			// 1. Handle the Ordering natively inside the Enricher
			List<Post> orderedPosts = unorderedPosts;
			if (orderedIds != null && orderedIds.Count > 0)
			{
				var postMap = unorderedPosts.ToDictionary(p => p.Id);
				orderedPosts = orderedIds.Where(postMap.ContainsKey).Select(id => postMap[id]).ToList();
			}

			// 2. Fetch the Enrichment Data
			var postIds = orderedPosts.Select(p => p.Id).ToList();
			var authorIds = orderedPosts.Select(p => p.AuthorId).Distinct().ToList();

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

			// 3. Map to DTOs in the correct order
			return orderedPosts.Select(p => p.ToDto(
				authorUsernames.GetValueOrDefault(p.AuthorId, "Unknown"),
				likeCounts.GetValueOrDefault(p.Id, 0),
				commentCounts.GetValueOrDefault(p.Id, 0),
				likedPostIds.Contains(p.Id),
				bookmarkedPostIds.Contains(p.Id)
			)).ToList();
		}
	}
}