using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;

namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/follows")]
	[Authorize]
	public class FollowsController : ControllerBase
	{
		private readonly IFollowService _followService;

		public FollowsController(IFollowService followService)
		{
			_followService = followService;
		}

		private Guid? GetUserId()
		{
			var claim = User.FindFirst("sub")?.Value;
			return Guid.TryParse(claim, out var id) ? id : null;
		}

		[HttpPost("{userId:guid}")]
		public async Task<IActionResult> Follow(Guid userId)
		{
			var followerId = GetUserId();
			if (followerId == null) return Unauthorized();
			try
			{
				var result = await _followService.FollowAsync(followerId.Value, userId);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("{userId:guid}")]
		public async Task<IActionResult> Unfollow(Guid userId)
		{
			var followerId = GetUserId();
			if (followerId == null) return Unauthorized();
			try
			{
				await _followService.UnfollowAsync(followerId.Value, userId);
				return NoContent();
			}
			catch(KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}
	}
}
