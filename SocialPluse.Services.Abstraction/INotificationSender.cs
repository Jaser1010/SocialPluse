using SocialPluse.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface INotificationSender
	{
		Task SendAsync(Guid recipientId, NotificationDto dto);
	}
}
