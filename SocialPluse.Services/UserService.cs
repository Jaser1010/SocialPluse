using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Users;


namespace SocialPluse.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly AppDbContext _appDbContext;

		public UserService(UserManager<AppUser> userManager, AppDbContext appDbContext)
		{
			_userManager = userManager;
			_appDbContext = appDbContext;
		}

		private async Task<UserProfileDto> BuildUserProfileDtoAsync(AppUser user)
		{
			var postsCount = await _appDbContext.Posts.CountAsync(p => p.AuthorId == user.Id);
			var followersCount = await _appDbContext.Follows.CountAsync(f => f.FolloweeId == user.Id);
			var followingCount = await _appDbContext.Follows.CountAsync(f => f.FollowerId == user.Id);

			return new UserProfileDto
			{
				Id = user.Id.ToString(),
				Username = user.UserName ?? throw new InvalidOperationException("UserName is missing."),
				Email = user.Email ?? throw new InvalidOperationException("Email is missing."),
				DisplayName = user.DisplayName,
				Bio = user.Bio,
				AvatarUrl = user.AvatarUrl,
				PostsCount = postsCount,
				FollowersCount = followersCount,
				FollowingCount = followingCount,
				CreatedAt = user.CreatedAt
			};
		}

		public async Task<UserProfileDto> GetByUsernameAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			if (user == null)	throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)	throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
		{
			// 1. Find user by id 
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");
			// 2. Update only non-null fields
			if (request.DisplayName != null) user.DisplayName = request.DisplayName;
			if (request.Bio != null) user.Bio = request.Bio;
			if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
			// 3. Call UpdateAsync(user)
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to update profile: {errors}");
			}
			// 4. Return new UserProfileDto { ... }
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<List<UserRecommendationDto>> GetRecommendationsAsync(Guid userId, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 20);

			var followedIds = await _appDbContext.Follows
				.Where(f => f.FollowerId == userId)
				.Select(f => f.FolloweeId)
				.ToListAsync();

			followedIds.Add(userId);

			var recommendations = await _userManager.Users
				.Where(u => !followedIds.Contains(u.Id))
				.OrderByDescending(u => u.CreatedAt)
				.Take(clampedLimit)
				.Select(u => new UserRecommendationDto
				{
					Id = u.Id.ToString(),
					Username = u.UserName ?? string.Empty,
					DisplayName = u.DisplayName,
					AvatarUrl = u.AvatarUrl
				})
				.ToListAsync();

			return recommendations;
		}

		public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
				throw new InvalidOperationException("Current and new password are required.");

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
				throw new KeyNotFoundException("User not found.");

			var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to change password: {errors}");
			}
		}




		public async Task<bool> UserExistsAsync(Guid userId)
		{
			return await _userManager.Users.AnyAsync(u => u.Id == userId);
		}

		public async Task<string?> GetUsernameAsync(Guid userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			return user?.UserName;
		}

		public async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds)
		{
			var idsList = userIds.ToList();
			return await _userManager.Users
				.Where(u => idsList.Contains(u.Id))
				.ToDictionaryAsync(u => u.Id, u => u.UserName!);
		}
	}
}
