using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;


namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/search")]
	public class SearchController : ControllerBase
	{
		private readonly ISearchService _searchService;

		public SearchController(ISearchService searchService)
		{
			_searchService = searchService;
		}




		[HttpGet("posts")]
		public async Task<IActionResult> SearchPosts([FromQuery] string q, [FromQuery] int limit = 20)
		{
			if (string.IsNullOrWhiteSpace(q))
				return BadRequest(new { message = "Search query cannot be empty." });

			var result = await _searchService.SearchPostsAsync(q, limit);
			return Ok(result);
		}




		[HttpGet("users")]
		public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int limit = 20)
		{
			if (string.IsNullOrWhiteSpace(q))
				return BadRequest(new { message = "Search query cannot be empty." });

			var result = await _searchService.SearchUsersAsync(q, limit);
			return Ok(result);
		}

	}
}
