using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Safety;
using SocialPluse.Web.Extensions;


namespace SocialPluse.Web.Controllers
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






		[HttpPost]
		public async Task<IActionResult> ReportUser([FromBody] CreateReportRequest request)
		{
			var result = await _safetyService.CreateReportAsync(User.GetUserId(), request);
			return Ok(result);
		}

		[HttpGet("me")]
		public async Task<IActionResult> GetMyReports()
		{
			return Ok(await _safetyService.GetMyReportsAsync(User.GetUserId()));
		}
	}
}
