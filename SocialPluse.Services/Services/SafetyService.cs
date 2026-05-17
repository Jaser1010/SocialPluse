using Microsoft.Extensions.Logging;
using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Extensions;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Safety;

namespace SocialPluse.Services.Services
{
	public class SafetyService : ISafetyService
	{
		private readonly ISafetyRepository _safetyRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;
		private readonly ILogger<SafetyService> _logger;

		public SafetyService(
			ISafetyRepository safetyRepository,
			IBackgroundJobPublisher jobPublisher,
			ILogger<SafetyService> logger)
		{
			_safetyRepository = safetyRepository;
			_jobPublisher = jobPublisher;
			_logger = logger;
		}

		public async Task<BlockResponse> BlockUserAsync(Guid blockerId, Guid blockedId)
		{
			if (blockerId == blockedId)
			{
				_logger.LogWarning("Security: User {BlockerId} attempted to block themselves.", blockerId);
				throw new InvalidOperationException("Cannot block yourself.");
			}

			var existingBlock = await _safetyRepository.GetBlockAsync(blockerId, blockedId);
			if (existingBlock != null) throw new InvalidOperationException("Already blocked.");

			var block = new Block { BlockerId = blockerId, BlockedId = blockedId, CreatedAt = DateTime.UtcNow };

			await _safetyRepository.AddBlockAsync(block);
			await _safetyRepository.SaveChangesAsync();

			_logger.LogInformation("User {BlockerId} successfully blocked User {BlockedId}", blockerId, blockedId);

			return block.ToResponse();
		}

		public async Task UnblockUserAsync(Guid blockerId, Guid blockedId)
		{
			var block = await _safetyRepository.GetBlockAsync(blockerId, blockedId);
			if (block == null) throw new KeyNotFoundException("Block relationship not found.");

			_safetyRepository.RemoveBlock(block);
			await _safetyRepository.SaveChangesAsync();

			_logger.LogInformation("User {BlockerId} unblocked User {BlockedId}", blockerId, blockedId);
		}

		public async Task<MuteResponse> MuteUserAsync(Guid muterId, Guid mutedId)
		{
			if (muterId == mutedId) throw new InvalidOperationException("Cannot mute yourself.");

			var existingMute = await _safetyRepository.GetMuteAsync(muterId, mutedId);
			if (existingMute != null) throw new InvalidOperationException("Already muted.");

			var mute = new Mute { MuterId = muterId, MutedId = mutedId, CreatedAt = DateTime.UtcNow };

			await _safetyRepository.AddMuteAsync(mute);
			await _safetyRepository.SaveChangesAsync();

			_logger.LogInformation("User {MuterId} muted User {MutedId}", muterId, mutedId);

			return mute.ToResponse();
		}

		public async Task UnmuteUserAsync(Guid muterId, Guid mutedId)
		{
			var mute = await _safetyRepository.GetMuteAsync(muterId, mutedId);
			if (mute == null) throw new KeyNotFoundException("Mute relationship not found.");

			_safetyRepository.RemoveMute(mute);
			await _safetyRepository.SaveChangesAsync();

			_logger.LogInformation("User {MuterId} unmuted User {MutedId}", muterId, mutedId);
		}

		public async Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request)
		{
			if (request.TargetType.ToLower() == "user" && request.TargetId == reporterId)
			{
				_logger.LogWarning("Security Alert: User {ReporterId} attempted to report themselves.", reporterId);
				throw new InvalidOperationException("You cannot report yourself.");
			}

			var targetType = request.TargetType.ToLower();
			if (targetType != "user" && targetType != "post")
				throw new InvalidOperationException("Invalid TargetType. Must be 'user' or 'post'.");

			var reason = request.Reason?.Sanitize();
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

				_logger.LogInformation("User {ReporterId} created a {TargetType} report against Target {TargetId}",
					reporterId, targetType, request.TargetId);

				return report.ToDto();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Transaction rolled back while User {ReporterId} attempted to submit a report.", reporterId);
				throw;
			}
		}

		public async Task<List<ReportDto>> GetMyReportsAsync(Guid reporterId)
		{
			var reports = await _safetyRepository.GetMyReportsAsync(reporterId);
			return reports.Select(r => r.ToDto()).ToList();
		}
	}
}