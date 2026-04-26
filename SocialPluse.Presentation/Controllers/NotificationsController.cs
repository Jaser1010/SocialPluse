using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;

namespace SocialPluse.Presentation.Controllers
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


		private Guid? GetUserId()
		{
			var claim = User.FindFirst("sub")?.Value;
			return Guid.TryParse(claim, out var id) ? id : null;
		}

		[HttpGet]
		public async Task<IActionResult> GetNotifications([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			var notifications = await _notificationService.GetNotificationsAsync(userId.Value, cursor, limit);
			return Ok(notifications);
		}


		[HttpPost("{id:guid}/read")]
		public async Task<IActionResult> MarkAsRead(Guid id)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				await _notificationService.MarkAsReadAsync(id, userId.Value);
				return NoContent();
			}
			catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
			catch (UnauthorizedAccessException) { return Forbid(); }
		}
	}
}
