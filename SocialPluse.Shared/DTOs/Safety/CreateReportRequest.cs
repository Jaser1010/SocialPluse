

namespace SocialPluse.Shared.DTOs.Safety
{
	public class CreateReportRequest
	{
		public string TargetType { get; set; } = string.Empty; // "user" | "post"
		public Guid TargetId { get; set; }
		public string Reason { get; set; } = string.Empty;
	}
}
