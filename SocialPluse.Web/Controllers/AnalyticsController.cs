using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api/analytics")]
	public class AnalyticsController : ControllerBase
	{
		private readonly IAnalyticsService _analyticsService;

		public AnalyticsController(IAnalyticsService analyticsService)
		{
			_analyticsService = analyticsService;
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> GetMyAnalytics()
		{
			var stats = await _analyticsService.GetUserAnalyticsAsync(User.GetUserId());
			return Ok(stats);
		}

		[HttpGet("trending")]
		public async Task<IActionResult> GetTrendingTopics([FromQuery] int limit = 8, [FromQuery] int hours = 72)
		{
			var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : Guid.Empty;
			var trending = await _analyticsService.GetTrendingTopicsAsync(userId, limit, hours);
			return Ok(trending);
		}
	}
}