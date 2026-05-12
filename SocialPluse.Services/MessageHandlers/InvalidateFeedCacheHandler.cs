using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class InvalidateFeedCacheHandler : IOutboxMessageHandler
	{
		private readonly IFeedService _feedService;

		public InvalidateFeedCacheHandler(IFeedService feedService)
		{
			_feedService = feedService;
		}

		public string HandledMessageType => OutboxMessageTypes.InvalidateFeedCache;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var userId = payload.RootElement.GetProperty("UserId").GetGuid();

			await _feedService.InvalidateFeedCacheAsync(userId);
		}
	}
}