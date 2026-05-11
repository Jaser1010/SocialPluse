using Microsoft.Extensions.DependencyInjection;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IFollowService, FollowService>();
			services.AddScoped<ILikeService, LikeService>();
			services.AddScoped<ICommentService, CommentService>();
			services.AddScoped<INotificationService, NotificationService>();
			services.AddScoped<ISearchService, SearchService>();
			services.AddScoped<ISafetyService, SafetyService>();

			services.AddScoped<IPostService, PostService>();
			services.AddScoped<IFeedService, FeedService>();
			services.AddScoped<IBookmarkService, BookmarkService>();
			services.AddScoped<IAnalyticsService, AnalyticsService>();
			return services;
		}
	}
}
