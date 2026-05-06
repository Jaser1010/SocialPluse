using SocialPluse.Shared.DTOs.Search;


namespace SocialPluse.Services.Abstraction.IService
{
	public interface ISearchService
	{
		Task<SearchPostsResponse> SearchPostsAsync(string query, int limit);
		Task<SearchUsersResponse> SearchUsersAsync(string query, int limit);
	}
}
