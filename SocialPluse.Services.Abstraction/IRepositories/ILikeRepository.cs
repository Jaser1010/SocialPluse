using SocialPluse.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface ILikeRepository
	{
		Task<Guid?> GetPostAuthorIdAsync(Guid postId);
		Task<Like?> GetLikeAsync(Guid userId, Guid postId);
		Task AddAsync(Like like);
		void Remove(Like like);
		Task<int> SaveChangesAsync();
	}
}