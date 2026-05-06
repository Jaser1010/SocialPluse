using System.Security.Claims;

namespace SocialPluse.Web.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static Guid GetUserId(this ClaimsPrincipal user)
		{
			var claim = user.FindFirst("sub")?.Value;

			if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var userId))
			{
				// In an [Authorize] controller, this should theoretically never be hit
				throw new UnauthorizedAccessException("User identification is missing or invalid.");
			}

			return userId;
		}
	}
}