using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api/media")]
	public class MediaController : ControllerBase
	{
		private readonly IMediaService _mediaService;

		// The controller only knows about the interface!
		public MediaController(IMediaService mediaService)
		{
			_mediaService = mediaService;
		}

		[Authorize]
		[HttpPost("upload")]
		[RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
		public async Task<IActionResult> Upload([FromForm] IFormFile file)
		{
			// The GlobalExceptionMiddleware will catch ArgumentException and return 400
			var url = await _mediaService.UploadFileAsync(file);

			return Ok(new { url });
		}
	}
}