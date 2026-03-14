using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;


namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/blocks")]
	[Authorize]
	public class BlocksController : ControllerBase
	{
		private readonly ISafetyService _safetyService;

		public BlocksController(ISafetyService safetyService)
		{
			_safetyService = safetyService;
		}

		private Guid GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst("sub")?.Value;

			return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
		}

		[HttpPost("{userId:guid}")]
		public async Task<IActionResult> BlockUser(Guid userId)
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty)	return Unauthorized();
			try
			{
				var result = await _safetyService.BlockUserAsync(currentUserId, userId);
				return Ok(result);
			}
			catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
			catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }

		}



		[HttpDelete("{userId:guid}")]
		public async Task<IActionResult> UnblockUser(Guid userId)
		{
			var currentUserId = GetCurrentUserId();
			if (currentUserId == Guid.Empty)	return Unauthorized();
			try
			{
				await _safetyService.UnblockUserAsync(currentUserId, userId);
				return NoContent();
			}
			catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
		}
	}
}
