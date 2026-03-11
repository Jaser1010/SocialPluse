using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Safety;


namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/reports")]
	[Authorize]
	public class ReportsController : ControllerBase
	{
		private readonly ISafetyService _safetyService;

		public ReportsController(ISafetyService safetyService)
		{
			_safetyService = safetyService;
		}


		private Guid GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst("sub")?.Value;
			return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
		}



		[HttpPost]
		public async Task<IActionResult> ReportUser([FromBody] CreateReportRequest request)
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty)	return Unauthorized();
			var result = await _safetyService.CreateReportAsync(currentUserId, request); 
			return Ok(result);
		}

		[HttpGet("me")]
		public async Task<IActionResult> GetMyReports()
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty) return Unauthorized();

			var reports = await _safetyService.GetMyReportsAsync(currentUserId);
			return Ok(reports);
		}
	}
}
