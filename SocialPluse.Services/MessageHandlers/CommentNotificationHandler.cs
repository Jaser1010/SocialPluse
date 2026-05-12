using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class CommentNotificationHandler : IOutboxMessageHandler
	{
		private readonly INotificationService _notificationService;

		public CommentNotificationHandler(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public string HandledMessageType => OutboxMessageTypes.CreateCommentNotification;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var postAuthorId = payload.RootElement.GetProperty("PostAuthorId").GetGuid();
			var commentAuthorId = payload.RootElement.GetProperty("CommentAuthorId").GetGuid();
			var postId = payload.RootElement.GetProperty("PostId").GetGuid();
			var commentId = payload.RootElement.GetProperty("CommentId").GetGuid();

			await _notificationService.CreateCommentNotificationAsync(postAuthorId, commentAuthorId, postId, commentId);
		}
	}
}