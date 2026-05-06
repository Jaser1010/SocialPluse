using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
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

		

		[HttpPost]
		[HttpPost]
		public async Task<IActionResult> LikePost(Guid postId)
		{
			var result = await _likeService.LikePostAsync(User.GetUserId(), postId);
			return Ok(result);
		}

		[HttpDelete]
		public async Task<IActionResult> UnlikePost(Guid postId)
		{
			await _likeService.UnlikePostAsync(User.GetUserId(), postId);
			return NoContent();
		}
	}
}
