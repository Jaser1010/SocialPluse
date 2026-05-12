using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IAnalyticsService
	{
		Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId);
		Task<List<TrendingTopicDto>> GetTrendingTopicsAsync(Guid userId, int limit = 8, int hours = 72);
	}
}