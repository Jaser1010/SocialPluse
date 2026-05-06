using SocialPluse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface ICommentRepository
	{
		Task<bool> PostExistsAsync(Guid postId);
		Task<Guid?> GetPostAuthorIdAsync(Guid postId);
		Task AddAsync(Comment comment);
		Task<int> SaveChangesAsync();
		Task<List<Comment>> GetCommentsAsync(Guid postId, DateTime? cursor, int limit);
	}
}
