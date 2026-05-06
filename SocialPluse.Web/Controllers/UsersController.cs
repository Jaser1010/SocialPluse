using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Users;
using SocialPluse.Web.Extensions;
using System.Security.Claims;

namespace SocialPluse.Web.Controllers
{
	[ApiController]

	[Route("api/users")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}


		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> GetMe()
		{
			return Ok(await _userService.GetCurrentUserAsync(User.GetUserId()));
		}

		[Authorize]
		[HttpPut("me")]
		public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
		{
			return Ok(await _userService.UpdateProfileAsync(User.GetUserId(), request));
		}

		[Authorize]
		[HttpGet("recommendations")]
		public async Task<IActionResult> GetRecommendations([FromQuery] int limit = 3)
		{
			return Ok(await _userService.GetRecommendationsAsync(User.GetUserId(), limit));
		}

		[Authorize]
		[HttpPost("me/change-password")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			await _userService.ChangePasswordAsync(User.GetUserId(), request);
			return NoContent();
		}

	}
}
