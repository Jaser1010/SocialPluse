using SocialPluse.Shared.DTOs.Search;


namespace SocialPluse.Services.Abstraction
{
	public interface ISearchService
	{
		Task<SearchPostsResponse> SearchPostsAsync(string query, int limit);
		Task<SearchUsersResponse> SearchUsersAsync(string query, int limit);
	}
}
