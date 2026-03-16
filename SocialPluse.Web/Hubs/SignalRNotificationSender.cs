using Microsoft.AspNetCore.SignalR;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Notifications;

namespace SocialPluse.Web.Hubs
{
	public class SignalRNotificationSender : INotificationSender
	{
		private readonly IHubContext<NotificationHub> _hubContext;
		public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
			=> _hubContext = hubContext;

		public async Task SendAsync(Guid recipientId, NotificationDto dto)
			=> await _hubContext.Clients
				.Group($"user_{recipientId}")
				.SendAsync("ReceiveNotification", dto);
	}
}
