using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api/notifications")]
	[Authorize]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _notificationService;

		public NotificationsController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}




		[HttpGet]
		public async Task<IActionResult> GetNotifications([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			var notifications = await _notificationService.GetNotificationsAsync(User.GetUserId(), cursor, limit);
			return Ok(notifications);
		}

		[HttpPost("{id:guid}/read")]
		public async Task<IActionResult> MarkAsRead(Guid id)
		{
			await _notificationService.MarkAsReadAsync(id, User.GetUserId());
			return NoContent();
		}
	}
}
