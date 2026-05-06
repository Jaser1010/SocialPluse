using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api/media")]
	public class MediaController : ControllerBase
	{
		private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg", ".jpeg", ".png", ".webp", ".gif"
		};

		[Authorize]
		[HttpPost("upload")]
		[RequestSizeLimit(10 * 1024 * 1024)]
		public async Task<IActionResult> Upload([FromForm] IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new { message = "No file uploaded." });

			var extension = Path.GetExtension(file.FileName);
			if (!AllowedExtensions.Contains(extension))
				return BadRequest(new { message = "Unsupported file type." });

			var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
			Directory.CreateDirectory(uploadsDir);

			var fileName = $"{Guid.NewGuid():N}{extension}";
			var physicalPath = Path.Combine(uploadsDir, fileName);

			await using (var stream = new FileStream(physicalPath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
			return Ok(new { url });
		}
	}
}
