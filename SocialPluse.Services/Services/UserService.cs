using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Users;
using SocialPluse.Services.Extensions;

namespace SocialPluse.Services.Services
{
	public class UserService : IUserService
	{
		private readonly IIdentityWrapper _identityWrapper;
		private readonly IPostRepository _postRepository;
		private readonly IFollowRepository _followRepository;
		private readonly IUserRepository _userRepository;

		public UserService(
			IIdentityWrapper identityWrapper,
			IPostRepository postRepository,
			IFollowRepository followRepository,
			IUserRepository userRepository)
		{
			_identityWrapper = identityWrapper;
			_postRepository = postRepository;
			_followRepository = followRepository;
			_userRepository = userRepository;
		}

		private async Task<UserProfileDto> BuildUserProfileDtoAsync(UserDetailsDto user)
		{
			var postsCount = await _postRepository.GetPostCountAsync(user.Id);
			var (followersCount, followingCount) = await _followRepository.GetFollowStatsAsync(user.Id);

			return new UserProfileDto
			{
				Id = user.Id.ToString(),
				Username = user.Username,
				Email = user.Email,
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
			var user = await _identityWrapper.GetUserByUsernameAsync(username);
			if (user == null) throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
		{
			var user = await _identityWrapper.GetUserByIdAsync(userId);
			if (user == null) throw new KeyNotFoundException("User not found.");
			return await BuildUserProfileDtoAsync(user);
		}

		public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
		{
			// Sanitize user-provided text at the service boundary
			var cleanDisplayName = request.DisplayName?.Sanitize();
			var cleanBio = request.Bio?.Sanitize();

			await _identityWrapper.UpdateProfileAsync(userId, cleanDisplayName, cleanBio, request.AvatarUrl);

			var updatedUser = await _identityWrapper.GetUserByIdAsync(userId);
			return await BuildUserProfileDtoAsync(updatedUser!);
		}

		public async Task<List<UserRecommendationDto>> GetRecommendationsAsync(Guid userId, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 20);
			var recommendedIds = await _followRepository.GetRecommendedUserIdsAsync(userId, clampedLimit);

			if (recommendedIds.Count == 0) return [];

			var users = await _identityWrapper.GetUsersByIdsAsync(recommendedIds);

			return users.Select(user => new UserRecommendationDto
			{
				Id = user.Id.ToString(),
				Username = user.Username,
				DisplayName = user.DisplayName,
				AvatarUrl = user.AvatarUrl
			}).ToList();
		}

		public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
				throw new InvalidOperationException("Current and new password are required.");

			await _identityWrapper.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
		}

		public async Task<bool> UserExistsAsync(Guid userId) => await _userRepository.UserExistsAsync(userId);

		public async Task<string?> GetUsernameAsync(Guid userId) => await _userRepository.GetUsernameAsync(userId);

		public async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds) => await _userRepository.GetUsernamesAsync(userIds);
	}
}