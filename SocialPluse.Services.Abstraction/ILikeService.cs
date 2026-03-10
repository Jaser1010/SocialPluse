using SocialPluse.Shared.DTOs.Likes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface ILikeService
	{
		Task<LikeResponse> LikePostAsync(Guid userId, Guid postId);
		Task UnlikePostAsync(Guid userId, Guid postId);
	}
}
