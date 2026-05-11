using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Persistence.Repositories
{
	public class SearchRepository : ISearchRepository
	{
		private readonly AppDbContext _context;

		public SearchRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<List<Post>> SearchPostsAsync(string query, int limit)
		{
			return await _context.Posts
				.AsNoTracking()
				.Where(p => p.SearchVector != null &&
							p.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("english", query)))
				.OrderByDescending(p => p.CreatedAt)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<List<UserProfileDto>> SearchUsersAsync(string query, int limit)
		{
			return await _context.Users
				.Where(u => EF.Functions.ILike(u.UserName!, $"%{query}%"))
				.Take(limit)
				.Select(u => new UserProfileDto
				{
					Id = u.Id.ToString(),
					Username = u.UserName!,
					Email = u.Email!,
					DisplayName = u.DisplayName,
					Bio = u.Bio,
					AvatarUrl = u.AvatarUrl,
					CreatedAt = u.CreatedAt
				})
				.ToListAsync();
		}
	}
}