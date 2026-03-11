

namespace SocialPluse.Shared.DTOs.Safety
{
	public class ReportDto
	{
		public Guid Id { get; set; }
		public Guid ReporterId { get; set; }
		public string TargetType { get; set; } = string.Empty;
		public Guid TargetId { get; set; }
		public string Reason { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}
