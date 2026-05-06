using SocialPluse.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface IFollowRepository
	{
		Task<Follow?> GetFollowAsync(Guid followerId, Guid followeeId);
		Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
		Task AddAsync(Follow follow);
		void Remove(Follow follow);
		Task<int> SaveChangesAsync();
	}
}