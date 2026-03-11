using Microsoft.AspNetCore.Identity;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Users;


namespace SocialPluse.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> _userManager;

		public UserService(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<UserProfileDto> GetByUsernameAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			if (user == null)	throw new KeyNotFoundException("User not found.");
			return new UserProfileDto
			{
				Id = user.Id.ToString(),
				Username = user.UserName ?? throw new InvalidOperationException("UserName is missing."),
				Email = user.Email ?? throw new InvalidOperationException("Email is missing."),
				DisplayName = user.DisplayName,
				Bio = user.Bio,
				AvatarUrl = user.AvatarUrl,
				CreatedAt = user.CreatedAt
			};
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)	throw new KeyNotFoundException("User not found.");
			return new UserProfileDto
			{
				Id = user.Id.ToString(),
				Username = user.UserName ?? throw new InvalidOperationException("UserName is missing."),
				Email = user.Email ?? throw new InvalidOperationException("Email is missing."),
				DisplayName = user.DisplayName,
				Bio = user.Bio,
				AvatarUrl = user.AvatarUrl,
				CreatedAt = user.CreatedAt
			};
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
			return new UserProfileDto
			{
				Id = user.Id.ToString(),
				Username = user.UserName ?? throw new InvalidOperationException("UserName is missing."),
				Email = user.Email ?? throw new InvalidOperationException("Email is missing."),
				DisplayName = user.DisplayName,
				Bio = user.Bio,
				AvatarUrl = user.AvatarUrl,
				CreatedAt = user.CreatedAt
			};
		}
	}
}
