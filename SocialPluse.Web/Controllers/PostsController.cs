using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api/posts")]
	public class PostsController : ControllerBase
	{
		private readonly IPostService _postService;

		public PostsController(IPostService postService)
		{
			_postService = postService;
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
		{
			var post = await _postService.CreatePostAsync(User.GetUserId(), request);
			return CreatedAtAction(nameof(GetPost), new { postId = post.Id }, post);
		}

		[HttpGet("{postId:guid}")]
		public async Task<IActionResult> GetPost(Guid postId)
		{
			// If the user is logged in, pass their ID to check if they liked/bookmarked it
			var currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
			var post = await _postService.GetByIdAsync(postId, currentUserId);
			return Ok(post);
		}

		[Authorize]
		[HttpDelete("{postId:guid}")]
		public async Task<IActionResult> DeletePost(Guid postId)
		{
			await _postService.DeletePostAsync(postId, User.GetUserId());
			return NoContent(); // 204 No Content is standard for successful deletions
		}
	}
}