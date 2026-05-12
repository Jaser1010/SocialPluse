using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class LikeNotificationHandler : IOutboxMessageHandler
	{
		private readonly INotificationService _notificationService;

		public LikeNotificationHandler(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public string HandledMessageType => OutboxMessageTypes.CreateLikeNotification;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var recipientId = payload.RootElement.GetProperty("RecipientId").GetGuid();
			var actorId = payload.RootElement.GetProperty("ActorId").GetGuid();
			var postId = payload.RootElement.GetProperty("PostId").GetGuid();

			await _notificationService.CreateLikeNotificationAsync(recipientId, actorId, postId);
		}
	}
}