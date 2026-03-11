using Microsoft.AspNetCore.SignalR;

namespace SocialPluse.Web.Hubs
{
	public class SubClaimUserIdProvider : IUserIdProvider
	{
			public string? GetUserId(HubConnectionContext connection)
				=> connection.User?.FindFirst("sub")?.Value;
	}
}
