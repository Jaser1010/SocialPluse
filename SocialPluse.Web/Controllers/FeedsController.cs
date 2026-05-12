using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api/feed")]
	[Authorize] // Viewing the personalized feed requires authentication
	public class FeedsController : ControllerBase
	{
		private readonly IFeedService _feedService;

		public FeedsController(IFeedService feedService)
		{
			_feedService = feedService;
		}

		[HttpGet]
		public async Task<IActionResult> GetFeed([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			// Uses your highly optimized Redis fallback method
			var result = await _feedService.GetFeedFromCacheAsync(User.GetUserId(), cursor, limit);
			return Ok(result);
		}
	}
}