

namespace SocialPluse.Domain.Entities
{
	public class Mute
	{
		public Guid MuterId { get; set; }
		public Guid MutedId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
