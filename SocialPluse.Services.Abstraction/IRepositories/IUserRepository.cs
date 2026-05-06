using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface IUserRepository
	{
		Task<bool> UserExistsAsync(Guid userId);
		Task<string?> GetUsernameAsync(Guid userId);
		Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds);
	}
}