using Microsoft.EntityFrameworkCore.Storage;
using SocialPluse.Domain.Entities;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface ICommentRepository
	{
		Task<bool> PostExistsAsync(Guid postId);
		Task<Guid?> GetPostAuthorIdAsync(Guid postId);
		Task AddAsync(Comment comment);
		Task<int> SaveChangesAsync();
		Task<List<Comment>> GetCommentsAsync(Guid postId, DateTime? cursor, int limit);
		Task<IDbContextTransaction> BeginTransactionAsync();
	}
}
