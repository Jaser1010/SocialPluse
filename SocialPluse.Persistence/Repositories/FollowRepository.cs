using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class FollowRepository : IFollowRepository
	{
		private readonly AppDbContext _context;

		public FollowRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<Follow?> GetFollowAsync(Guid followerId, Guid followeeId)
		{
			return await _context.Follows.FindAsync(followerId, followeeId);
		}

		public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
		{
			return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
		}

		public async Task AddAsync(Follow follow)
		{
			await _context.Follows.AddAsync(follow);
		}

		public void Remove(Follow follow)
		{
			_context.Follows.Remove(follow);
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}