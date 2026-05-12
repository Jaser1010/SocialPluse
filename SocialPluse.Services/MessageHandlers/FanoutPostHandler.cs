using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class FanoutPostHandler : IOutboxMessageHandler
	{
		private readonly IFeedService _feedService;

		public FanoutPostHandler(IFeedService feedService)
		{
			_feedService = feedService;
		}

		public string HandledMessageType => OutboxMessageTypes.FanoutPostToFeed;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var postId = payload.RootElement.GetProperty("PostId").GetGuid();
			var authorId = payload.RootElement.GetProperty("AuthorId").GetGuid();

			await _feedService.FanoutPostToFeedAsync(postId, authorId);
		}
	}
}