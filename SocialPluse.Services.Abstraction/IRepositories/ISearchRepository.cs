using SocialPluse.Domain.Entities;
using SocialPluse.Shared.DTOs.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface ISearchRepository
	{
		Task<List<Post>> SearchPostsAsync(string query, int limit);
		Task<List<UserProfileDto>> SearchUsersAsync(string query, int limit);
	}
}