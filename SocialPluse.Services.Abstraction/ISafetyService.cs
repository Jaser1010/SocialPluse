using SocialPluse.Shared.DTOs.Safety;


namespace SocialPluse.Services.Abstraction
{
	public interface ISafetyService
	{
		// Blocks
		Task<BlockResponse> BlockUserAsync(Guid blockerId, Guid blockedId);
		Task UnblockUserAsync(Guid blockerId, Guid blockedId);

		// Mutes
		Task<MuteResponse> MuteUserAsync(Guid muterId, Guid mutedId);
		Task UnmuteUserAsync(Guid muterId, Guid mutedId);

		// Reports
		Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request);
		Task<List<ReportDto>> GetMyReportsAsync(Guid reporterId);
	}
}
