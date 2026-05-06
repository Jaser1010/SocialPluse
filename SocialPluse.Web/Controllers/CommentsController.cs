using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Comments;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
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



		[Authorize]
		[HttpPost]
		public async Task<IActionResult> AddComment(Guid postId, [FromBody] CreateCommentRequest request)
		{
			var result = await _commentService.CreateCommentAsync(User.GetUserId(), postId, request);
			return Ok(result);
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
