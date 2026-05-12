using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IPostService
	{
		Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request);
		Task<PostDto> GetByIdAsync(Guid postId, Guid? currentUserId = null);
		Task DeletePostAsync(Guid postId, Guid requestingUserId);
	}
}
