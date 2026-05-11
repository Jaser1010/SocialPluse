using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> UserExistsAsync(Guid userId)
		{
			// Lightning fast, no tracking overhead
			return await _context.Users.AnyAsync(u => u.Id == userId);
		}

		public async Task<string?> GetUsernameAsync(Guid userId)
		{
			return await _context.Users
				.Where(u => u.Id == userId)
				.Select(u => u.UserName)
				.FirstOrDefaultAsync();
		}

		public async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds)
		{
			return await _context.Users
				.Where(u => userIds.Contains(u.Id))
				.ToDictionaryAsync(u => u.Id, u => u.UserName!);
		}
	}
}