using System.Text.RegularExpressions;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services
{
	public class AnalyticsService : IAnalyticsService
	{
		private readonly IPostRepository _postRepository;

		public AnalyticsService(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<UserAnalyticsDto> GetUserAnalyticsAsync(Guid userId)
		{
			return await _postRepository.GetUserAnalyticsAsync(userId);
		}

		public async Task<List<TrendingTopicDto>> GetTrendingTopicsAsync(Guid userId, int limit = 8, int hours = 72)
		{
			var clampedLimit = Math.Clamp(limit, 1, 20);
			var from = DateTime.UtcNow.AddHours(-Math.Clamp(hours, 1, 168));

			var postTexts = await _postRepository.GetRecentPostTextsAsync(userId, from, 3000);
			var hashtagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var text in postTexts)
			{
				if (string.IsNullOrWhiteSpace(text)) continue;
				var matches = Regex.Matches(text, @"#([A-Za-z0-9_]{2,50})");
				foreach (Match match in matches)
				{
					var tag = "#" + match.Groups[1].Value;
					hashtagCounts[tag] = hashtagCounts.TryGetValue(tag, out var current) ? current + 1 : 1;
				}
			}

			return hashtagCounts.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key).Take(clampedLimit)
				.Select(kvp => new TrendingTopicDto { Hashtag = kvp.Key, Mentions = kvp.Value }).ToList();
		}
	}
}