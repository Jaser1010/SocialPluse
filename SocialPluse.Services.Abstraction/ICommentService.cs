using SocialPluse.Shared.DTOs.Comments;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface ICommentService
	{
		Task<CommentDto> CreateCommentAsync(Guid authorId, Guid postId, CreateCommentRequest request);
		Task<CommentFeedResponse> GetCommentsAsync(Guid postId, DateTime? cursor, int limit);
	}
}
