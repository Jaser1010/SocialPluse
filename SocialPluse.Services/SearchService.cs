using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Search;
using SocialPluse.Services.Mappers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SocialPluse.Services
{
	public class SearchService : ISearchService
	{
		private readonly ISearchRepository _searchRepository;
		private readonly IUserRepository _userRepository;

		public SearchService(ISearchRepository searchRepository, IUserRepository userRepository)
		{
			_searchRepository = searchRepository;
			_userRepository = userRepository;
		}

		public async Task<SearchPostsResponse> SearchPostsAsync(string query, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			var posts = await _searchRepository.SearchPostsAsync(query, clampedLimit);

			var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
			var authors = await _userRepository.GetUsernamesAsync(authorIds);

			return new SearchPostsResponse
			{
				Posts = posts.Select(p => p.ToDto(
					authors.GetValueOrDefault(p.AuthorId, "Unknown"),
					0,
					0,
					false,
					false
				)).ToList()
			};
		}

		public async Task<SearchUsersResponse> SearchUsersAsync(string query, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);

			
			var users = await _searchRepository.SearchUsersAsync(query, clampedLimit);

			return new SearchUsersResponse
			{
				Users = users
			};
		}
	}
}