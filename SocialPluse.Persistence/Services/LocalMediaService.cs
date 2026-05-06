using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Persistence.Services
{
	public class LocalMediaService : IMediaService
	{
		private readonly string _uploadsFolder;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LocalMediaService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
		{
			// Define the physical path on the server
			_uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<string> UploadFileAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("No file uploaded.");

			// Ensure directory exists
			if (!Directory.Exists(_uploadsFolder))
				Directory.CreateDirectory(_uploadsFolder);

			var extension = Path.GetExtension(file.FileName);
			var fileName = $"{Guid.NewGuid():N}{extension}";
			var physicalPath = Path.Combine(_uploadsFolder, fileName);

			// Save the file
			await using (var stream = new FileStream(physicalPath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// Build the public URL
			var request = _httpContextAccessor.HttpContext?.Request;
			var baseUrl = $"{request?.Scheme}://{request?.Host}";

			return $"{baseUrl}/uploads/{fileName}";
		}
	}
}