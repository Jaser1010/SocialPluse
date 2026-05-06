using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IMediaService
	{
		Task<string> UploadFileAsync(IFormFile file);
	}
}