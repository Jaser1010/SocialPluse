using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class FollowNotificationHandler : IOutboxMessageHandler
	{
		private readonly INotificationService _notificationService;

		public FollowNotificationHandler(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public string HandledMessageType => OutboxMessageTypes.CreateFollowNotification;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var recipientId = payload.RootElement.GetProperty("RecipientId").GetGuid();
			var actorId = payload.RootElement.GetProperty("ActorId").GetGuid();

			await _notificationService.CreateFollowNotificationAsync(recipientId, actorId);
		}
	}
}