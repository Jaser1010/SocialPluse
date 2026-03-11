

namespace SocialPluse.Shared.DTOs.Safety
{
	public class MuteResponse
	{
		public Guid MuterId { get; set; }
		public Guid MutedId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
