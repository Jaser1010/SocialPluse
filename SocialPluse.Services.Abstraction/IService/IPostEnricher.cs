using SocialPluse.Domain.Entities;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IPostEnricher
	{
		Task<List<PostDto>> EnrichAndOrderPostsAsync(List<Post> unorderedPosts, List<Guid>? orderedIds = null, Guid? currentUserId = null);
	}
}