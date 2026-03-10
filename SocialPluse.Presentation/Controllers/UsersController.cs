using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Users;
using System.Security.Claims;

namespace SocialPluse.Presentation.Controllers
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
			try
			{
				var userIdClaim = User.FindFirst("sub")?.Value;

				if (string.IsNullOrWhiteSpace(userIdClaim))
					return Unauthorized();

				if (!Guid.TryParse(userIdClaim, out var userId))
					return Unauthorized();

				var result = await _userService.GetCurrentUserAsync(userId);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}


		[HttpGet("{username}")]
		public async Task<IActionResult> GetByUsername(string username)
		{
			try
			{
				var result = await _userService.GetByUsernameAsync(username);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}


		[Authorize]
		[HttpPut("me")]
		public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
		{
			try
			{
				// 1. Extract userId from claims
				var userIdClaim = User.FindFirst("sub")?.Value;
				if (string.IsNullOrWhiteSpace(userIdClaim))
					return Unauthorized();
				if (!Guid.TryParse(userIdClaim, out var userId))
					return Unauthorized();
				var result = await _userService.UpdateProfileAsync(userId, request);
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

	}
}
