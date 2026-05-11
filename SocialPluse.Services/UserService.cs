using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Users;
using SocialPluse.Services.Extensions;

namespace SocialPluse.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IPostRepository _postRepository;
		private readonly IFollowRepository _followRepository;
		private readonly IUserRepository _userRepository;

		public UserService(
			UserManager<AppUser> userManager,
			IPostRepository postRepository,
			IFollowRepository followRepository,
			IUserRepository userRepository)
		{
			_userManager = userManager;
			_postRepository = postRepository;
			_followRepository = followRepository;
			_userRepository = userRepository;
		}

		private async Task<UserProfileDto> BuildUserProfileDtoAsync(AppUser user)
		{
			// Use repositories for performance and decoupling
			var postsCount = await _postRepository.GetPostCountAsync(user.Id);
			var (followersCount, followingCount) = await _followRepository.GetFollowStatsAsync(user.Id);

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
			if (user == null) throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");

			// Hardening: Sanitize user-provided text to prevent XSS
			if (request.DisplayName != null) user.DisplayName = request.DisplayName.Sanitize();
			if (request.Bio != null) user.Bio = request.Bio.Sanitize();
			if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to update profile: {errors}");
			}

			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<List<UserRecommendationDto>> GetRecommendationsAsync(Guid userId, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 20);

			// Offload complex recommendation logic to the optimized repository
			var recommendedIds = await _followRepository.GetRecommendedUserIdsAsync(userId, clampedLimit);

			var recommendations = new List<UserRecommendationDto>();
			foreach (var id in recommendedIds)
			{
				var user = await _userManager.FindByIdAsync(id.ToString());
				if (user != null)
				{
					recommendations.Add(new UserRecommendationDto
					{
						Id = user.Id.ToString(),
						Username = user.UserName ?? string.Empty,
						DisplayName = user.DisplayName,
						AvatarUrl = user.AvatarUrl
					});
				}
			}

			return recommendations;
		}

		public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
				throw new InvalidOperationException("Current and new password are required.");

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null) throw new KeyNotFoundException("User not found.");

			var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Failed to change password: {errors}");
			}
		}

		public async Task<bool> UserExistsAsync(Guid userId)
		{
			// Efficient lookup via dedicated repository
			return await _userRepository.UserExistsAsync(userId);
		}

		public async Task<string?> GetUsernameAsync(Guid userId)
		{
			// Standardized repository access
			return await _userRepository.GetUsernameAsync(userId);
		}

		public async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds)
		{
			// Batch lookup via repository for optimal performance
			return await _userRepository.GetUsernamesAsync(userIds);
		}
	}
}