using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Comments;

namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/posts/{postId:guid}/comments")]
	public class CommentsController : ControllerBase
	{
		private readonly ICommentService _commentService;

		public CommentsController(ICommentService commentService)
		{
			_commentService = commentService;
		}

		private Guid? GetUserId()
		{
			var claim = User.FindFirst("sub")?.Value;
			return Guid.TryParse(claim, out var id) ? id : null;
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> AddComment(Guid postId, [FromBody] CreateCommentRequest request)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				var result = await _commentService.CreateCommentAsync(userId.Value, postId, request);
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

		[HttpGet]
		public async Task<IActionResult> GetComments(Guid postId, [FromQuery] DateTime? cursor, [FromQuery] int limit = 20)
		{
			try
			{
				var result = await _commentService.GetCommentsAsync(postId, cursor, limit);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}
	}
}
