using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Persistence.Services
{
	public class IdentityWrapper : IIdentityWrapper
	{
		private readonly UserManager<AppUser> _userManager;

		public IdentityWrapper(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<UserDetailsDto?> GetUserByIdAsync(Guid userId)
		{
			// FindByIdAsync always expects a string parameter by design in ASP.NET Identity
			var user = await _userManager.FindByIdAsync(userId.ToString());
			return user == null ? null : MapToDto(user);
		}

		public async Task<UserDetailsDto?> GetUserByUsernameAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			return user == null ? null : MapToDto(user);
		}

		public async Task<List<UserDetailsDto>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
		{
			var idsList = userIds.ToList();

			var users = await _userManager.Users
				.Where(u => idsList.Contains(u.Id)) 
				.ToListAsync();

			return users.Select(MapToDto).ToList();
		}

		public async Task UpdateProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");

			if (displayName != null) user.DisplayName = displayName;
			if (bio != null) user.Bio = bio;
			if (avatarUrl != null) user.AvatarUrl = avatarUrl;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to update profile: {errors}");
			}
		}

		public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");

			var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to change password: {errors}");
			}
		}

		private static UserDetailsDto MapToDto(AppUser user) => new()
		{
			Id = user.Id,
			Username = user.UserName ?? string.Empty,
			Email = user.Email ?? string.Empty,
			DisplayName = user.DisplayName,
			Bio = user.Bio,
			AvatarUrl = user.AvatarUrl,
			CreatedAt = user.CreatedAt
		};
	}
}