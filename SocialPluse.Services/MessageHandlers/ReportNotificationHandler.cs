using System.Text.Json;
using SocialPluse.Domain.Constants;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Services.MessageHandlers
{
	public class ReportNotificationHandler : IOutboxMessageHandler
	{
		private readonly INotificationService _notificationService;

		public ReportNotificationHandler(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public string HandledMessageType => OutboxMessageTypes.CreateReportNotification;

		public async Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken)
		{
			var targetId = payload.RootElement.GetProperty("TargetId").GetGuid();
			var reporterId = payload.RootElement.GetProperty("ReporterId").GetGuid();

			await _notificationService.CreateReportNotificationAsync(targetId, reporterId);
		}
	}
}