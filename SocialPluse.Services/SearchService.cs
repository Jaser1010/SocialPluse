using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Search;
using SocialPluse.Shared.DTOs.Users;


namespace SocialPluse.Services
{
	public class SearchService : ISearchService
	{
		private readonly AppDbContext _appDbContext;
		private readonly UserManager<AppUser> _userManager;

		public SearchService(AppDbContext appDbContext, UserManager<AppUser> userManager)
		{
			_appDbContext = appDbContext;
			_userManager = userManager;
		}
		public async Task<SearchPostsResponse> SearchPostsAsync(string query, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);

			var posts = await _appDbContext.Posts
				.Where(p => EF.Functions.ToTsVector("english", p.Text)
					.Matches(EF.Functions.WebSearchToTsQuery("english", query)))
				.OrderByDescending(p => p.CreatedAt)
				.Take(clampedLimit)
				.ToListAsync();

			// Batch fetch author usernames — same pattern as feed
			var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
			var authors = await _userManager.Users
				.Where(u => authorIds.Contains(u.Id))
				.ToDictionaryAsync(u => u.Id, u => u.UserName!);

			return new SearchPostsResponse
			{
				Posts = posts.Select(p => new PostDto
				{
					Id = p.Id,
					AuthorId = p.AuthorId,
					AuthorUsername = authors.GetValueOrDefault(p.AuthorId, "Unknown"),
					Text = p.Text,
					MediaUrl = p.MediaUrl,
					CreatedAt = p.CreatedAt
				}).ToList()
			};
		}

		public async Task<SearchUsersResponse> SearchUsersAsync(string query, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);

			var users = await _userManager.Users
				.Where(u => EF.Functions.ILike(u.UserName!, $"%{query}%"))
				.Take(clampedLimit)
				.ToListAsync();

			return new SearchUsersResponse
			{
				Users = users.Select(u => new UserProfileDto
				{
					Id = u.Id.ToString(),
					Username = u.UserName!,
					Email = u.Email!,
					DisplayName = u.DisplayName,
					Bio = u.Bio,
					AvatarUrl = u.AvatarUrl,
					CreatedAt = u.CreatedAt
				}).ToList()
			};
		}
	}
}
