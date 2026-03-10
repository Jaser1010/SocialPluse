using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;

namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/posts/{postId:guid}/likes")]
	[Authorize]
	public class LikesController : ControllerBase
	{
		private readonly ILikeService _likeService;

		public LikesController(ILikeService likeService)
		{
			_likeService = likeService;
		}

		private Guid? GetUserId()
		{
			var claim = User.FindFirst("sub")?.Value;
			return Guid.TryParse(claim, out var id) ? id : null;
		}

		[HttpPost]
		public async Task<IActionResult> LikePost(Guid postId)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				var result = await _likeService.LikePostAsync(userId.Value, postId);
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

		[HttpDelete]
		public async Task<IActionResult> UnlikePost(Guid postId)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				await _likeService.UnlikePostAsync(userId.Value, postId);
				return NoContent();
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}
	}
}
