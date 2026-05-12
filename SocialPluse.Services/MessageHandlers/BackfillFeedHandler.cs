using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class BackfillFeedHandler : IOutboxMessageHandler
	{
		private readonly IFeedService _feedService;

		public BackfillFeedHandler(IFeedService feedService)
		{
			_feedService = feedService;
		}

		public string HandledMessageType => OutboxMessageTypes.BackfillFolloweeFeed;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var followerId = payload.RootElement.GetProperty("FollowerId").GetGuid();
			var followeeId = payload.RootElement.GetProperty("FolloweeId").GetGuid();

			await _feedService.BackfillFolloweeFeedAsync(followerId, followeeId);
		}
	}
}