

namespace SocialPluse.Domain.Entities
{
	public class Report
	{
		public Guid Id { get; set; }
		public Guid ReporterId { get; set; }
		public string TargetType { get; set; } = string.Empty; // "user" | "post"
		public Guid TargetId { get; set; }
		public string Reason { get; set; } = string.Empty;
		public string Status { get; set; } = "pending"; // pending | reviewed | dismissed
		public DateTime CreatedAt { get; set; }
	}
}
