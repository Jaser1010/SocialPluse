using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Safety;
using SocialPluse.Services.Mappers;


namespace SocialPluse.Services
{
	public class SafetyService : ISafetyService
	{
		private readonly ISafetyRepository _safetyRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;

		public SafetyService(ISafetyRepository safetyRepository, IBackgroundJobPublisher jobPublisher)
		{
			_safetyRepository = safetyRepository;
			_jobPublisher = jobPublisher;
		}

		public async Task<BlockResponse> BlockUserAsync(Guid blockerId, Guid blockedId)
		{
			if (blockerId == blockedId) throw new InvalidOperationException("Cannot block yourself.");

			var existingBlock = await _safetyRepository.GetBlockAsync(blockerId, blockedId);
			if (existingBlock != null) throw new InvalidOperationException("Already blocked.");

			var block = new Block { BlockerId = blockerId, BlockedId = blockedId, CreatedAt = DateTime.UtcNow };

			await _safetyRepository.AddBlockAsync(block);
			await _safetyRepository.SaveChangesAsync();

			return block.ToResponse();
		}

		public async Task UnblockUserAsync(Guid blockerId, Guid blockedId)
		{
			var block = await _safetyRepository.GetBlockAsync(blockerId, blockedId);
			if (block == null) throw new KeyNotFoundException("Block relationship not found.");

			_safetyRepository.RemoveBlock(block);
			await _safetyRepository.SaveChangesAsync();
		}

		public async Task<MuteResponse> MuteUserAsync(Guid muterId, Guid mutedId)
		{
			if (muterId == mutedId) throw new InvalidOperationException("Cannot mute yourself.");

			var existingMute = await _safetyRepository.GetMuteAsync(muterId, mutedId);
			if (existingMute != null) throw new InvalidOperationException("Already muted.");

			var mute = new Mute { MuterId = muterId, MutedId = mutedId, CreatedAt = DateTime.UtcNow };

			await _safetyRepository.AddMuteAsync(mute);
			await _safetyRepository.SaveChangesAsync();

			return mute.ToResponse();
		}

		public async Task UnmuteUserAsync(Guid muterId, Guid mutedId)
		{
			var mute = await _safetyRepository.GetMuteAsync(muterId, mutedId);
			if (mute == null) throw new KeyNotFoundException("Mute relationship not found.");

			_safetyRepository.RemoveMute(mute);
			await _safetyRepository.SaveChangesAsync();
		}

		public async Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request)
		{
			var targetType = request.TargetType.ToLower();
			if (targetType != "user" && targetType != "post")
				throw new InvalidOperationException("Invalid TargetType. Must be 'user' or 'post'.");

			var reason = request.Reason?.Trim();
			if (string.IsNullOrEmpty(reason)) throw new InvalidOperationException("Reason cannot be empty.");

			var report = new Report
			{
				Id = Guid.NewGuid(),
				ReporterId = reporterId,
				TargetType = targetType,
				TargetId = request.TargetId,
				Reason = reason,
				Status = "pending",
				CreatedAt = DateTime.UtcNow
			};




			using var transaction = await _safetyRepository.BeginTransactionAsync();
			try
			{
				await _safetyRepository.AddReportAsync(report);
				await _safetyRepository.SaveChangesAsync();

				if (targetType == "user" && request.TargetId != reporterId)
					_jobPublisher.EnqueueReportNotificationJob(request.TargetId, reporterId);

				await transaction.CommitAsync();
				return report.ToDto();
			}
			catch { await transaction.RollbackAsync(); throw; }
		}

		public async Task<List<ReportDto>> GetMyReportsAsync(Guid reporterId)
		{
			var reports = await _safetyRepository.GetMyReportsAsync(reporterId);
			return reports.Select(r => r.ToDto()).ToList();
		}
	}
}