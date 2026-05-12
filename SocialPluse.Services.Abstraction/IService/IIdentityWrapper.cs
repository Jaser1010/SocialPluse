using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IIdentityWrapper
	{
		Task<UserDetailsDto?> GetUserByIdAsync(Guid userId);
		Task<UserDetailsDto?> GetUserByUsernameAsync(string username);
		Task<List<UserDetailsDto>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);

		Task UpdateProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl);
		Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
	}
}