using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
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



		[HttpPost("{userId:guid}")]
		public async Task<IActionResult> Follow(Guid userId)
		{
			var result = await _followService.FollowAsync(User.GetUserId(), userId);
			return Ok(result);
		}

		[HttpDelete("{userId:guid}")]
		public async Task<IActionResult> Unfollow(Guid userId)
		{
			await _followService.UnfollowAsync(User.GetUserId(), userId);
			return NoContent();
		}

		[HttpGet("{userId:guid}/status")]
		public async Task<IActionResult> GetFollowStatus(Guid userId)
		{
			var isFollowing = await _followService.IsFollowingAsync(User.GetUserId(), userId);
			return Ok(new { isFollowing });
		}
	}
}
