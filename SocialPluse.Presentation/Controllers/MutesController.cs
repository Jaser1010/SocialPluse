using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;


namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/mutes")]
	[Authorize]
	public class MutesController : ControllerBase
	{
		private readonly ISafetyService _safetyService;

		public MutesController(ISafetyService safetyService)
		{
			_safetyService = safetyService;
		}


		private Guid GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst("sub")?.Value;
			return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
		}


		[HttpPost("{userId:guid}")]
		public async Task<IActionResult> MuteUser(Guid userId)
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty)	return Unauthorized();
			var result = await _safetyService.MuteUserAsync(currentUserId, userId);
			return Ok(result);
		}

		[HttpDelete("{userId:guid}")]
		public async Task<IActionResult> UnmuteUser(Guid userId)
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty)	return Unauthorized();
			await _safetyService.UnmuteUserAsync(currentUserId, userId);
			return Ok("User unmuted successfully.");
		}
	}
}
