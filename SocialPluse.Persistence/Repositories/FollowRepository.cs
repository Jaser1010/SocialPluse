using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

		public async Task<IDbContextTransaction> BeginTransactionAsync() =>
					await _context.Database.BeginTransactionAsync();

		
		public async Task<(int Followers, int Following)> GetFollowStatsAsync(Guid userId)
		{
			var followers = await _context.Follows.CountAsync(f => f.FolloweeId == userId);
			var following = await _context.Follows.CountAsync(f => f.FollowerId == userId);
			return (followers, following);
		}

		public async Task<List<Guid>> GetRecommendedUserIdsAsync(Guid userId, int limit)
		{
			var myFollowing = _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId);

			return await _context.Follows
				.AsNoTracking()
				.Where(f => myFollowing.Contains(f.FollowerId) && f.FolloweeId != userId && !myFollowing.Contains(f.FolloweeId))
				.GroupBy(f => f.FolloweeId)
				.OrderByDescending(g => g.Count())
				.Select(g => g.Key)
				.Take(limit)
				.ToListAsync();
		}
	}
}