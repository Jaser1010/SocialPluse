using Microsoft.Extensions.DependencyInjection;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.MessageHandlers;
using SocialPluse.Services.Services;

namespace SocialPluse.Services
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			
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

			services.AddScoped<IPostEnricher, PostEnricher>();

			// Register all Outbox Message Handlers
			services.AddScoped<IOutboxMessageHandler, FanoutPostHandler>();
			services.AddScoped<IOutboxMessageHandler, LikeNotificationHandler>();
			services.AddScoped<IOutboxMessageHandler, CommentNotificationHandler>();
			services.AddScoped<IOutboxMessageHandler, FollowNotificationHandler>();
			services.AddScoped<IOutboxMessageHandler, BackfillFeedHandler>();
			services.AddScoped<IOutboxMessageHandler, InvalidateFeedCacheHandler>();
			services.AddScoped<IOutboxMessageHandler, ReportNotificationHandler>();

			return services;
		}
	}
}
