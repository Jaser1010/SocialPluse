using SocialPluse.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface IUserService
	{
		public Task<UserProfileDto> GetCurrentUserAsync(Guid userId);
		Task<UserProfileDto> GetByUsernameAsync(string username);
		Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
	}
}
