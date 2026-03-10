using Microsoft.Extensions.DependencyInjection;
using SocialPluse.Services.Abstraction;

namespace SocialPluse.Services
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IPostService, PostService>();
			services.AddScoped<IFollowService, FollowService>();
			return services;
		}
	}
}
