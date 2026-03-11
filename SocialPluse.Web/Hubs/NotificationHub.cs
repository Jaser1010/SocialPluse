using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialPluse.Web.Hubs
{
	[Authorize]
	public class NotificationHub : Hub
	{
		public override async Task OnConnectedAsync()
		{
			var userId = Context.UserIdentifier;
			await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
			await base.OnConnectedAsync();
		}
	}
}
