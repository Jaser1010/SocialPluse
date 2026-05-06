using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;


namespace SocialPluse.Web.Controllers
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



		[HttpPost("{userId:guid}")]
		public async Task<IActionResult> BlockUser(Guid userId)
		{
			var result = await _safetyService.BlockUserAsync(User.GetUserId(), userId);
			return Ok(result);
		}

		[HttpDelete("{userId:guid}")]
		public async Task<IActionResult> UnblockUser(Guid userId)
		{
			await _safetyService.UnblockUserAsync(User.GetUserId(), userId);
			return NoContent();
		}
	}
}
