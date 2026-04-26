using Hangfire;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Safety;

namespace SocialPluse.Services
{
	public class SafetyService : ISafetyService
	{
		private readonly AppDbContext _appDbContext;


		public SafetyService(AppDbContext appDbContext)
		{
			_appDbContext = appDbContext;
		}



		public async Task<BlockResponse> BlockUserAsync(Guid blockerId, Guid blockedId)
		{
			if (blockerId == blockedId) throw new InvalidOperationException("Cannot block yourself.");
			
			// 2. FindAsync(blockerId, blockedId) → if not null → InvalidOperationException("Already blocked.")
			var existingBlock = await _appDbContext.Blocks.FindAsync(blockerId, blockedId);
			if (existingBlock != null) throw new InvalidOperationException("Already blocked.");

			// 3. Create Block, Add, SaveChangesAsync
			var block = new Domain.Entities.Block
			{
				BlockerId = blockerId,
				BlockedId = blockedId,
				CreatedAt = DateTime.UtcNow
			};

			var entry = await _appDbContext.Blocks.AddAsync(block);
			await _appDbContext.SaveChangesAsync();
			// 4. Return BlockResponse
			return new BlockResponse
			{
				BlockerId = entry.Entity.BlockerId,
				BlockedId = entry.Entity.BlockedId,
				CreatedAt = entry.Entity.CreatedAt
			};
		}
		public async Task UnblockUserAsync(Guid blockerId, Guid blockedId)
		{
			// 1. FindAsync(blockerId, blockedId) → KeyNotFoundException if null
			var block = await _appDbContext.Blocks.FindAsync(blockerId, blockedId);
			if (block == null) throw new KeyNotFoundException("Block relationship not found.");
			// 2. Remove, SaveChangesAsync
			var entry = _appDbContext.Blocks.Remove(block);
			await _appDbContext.SaveChangesAsync();
		}




		public async Task<MuteResponse> MuteUserAsync(Guid muterId, Guid mutedId)
		{
			if (muterId == mutedId) throw new InvalidOperationException("Cannot mute yourself.");
			

			var existingMute = await _appDbContext.Mutes.FindAsync(muterId, mutedId);
			if (existingMute != null) throw new InvalidOperationException("Already muted.");


			var mute = new Domain.Entities.Mute
			{
				MuterId = muterId,
				MutedId = mutedId,
				CreatedAt = DateTime.UtcNow
			};

			var entry = await _appDbContext.Mutes.AddAsync(mute);
			
			await _appDbContext.SaveChangesAsync();

			return new MuteResponse
			{
				MuterId = entry.Entity.MuterId,
				MutedId = entry.Entity.MutedId,
				CreatedAt = entry.Entity.CreatedAt
			};

		}
		public async Task UnmuteUserAsync(Guid muterId, Guid mutedId)
		{
			var mute = await _appDbContext.Mutes.FindAsync(muterId, mutedId);
			if (mute == null) throw new KeyNotFoundException("Mute relationship not found.");

			var entry = _appDbContext.Mutes.Remove(mute);
			await _appDbContext.SaveChangesAsync();
		}






		public async Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request)
		{
			// 1. Validate TargetType is "user" or "post" → InvalidOperationException if not
			var TargetType = request.TargetType.ToLower();
			if (TargetType != "user" && TargetType != "post")
				throw new InvalidOperationException("Invalid TargetType. Must be 'user' or 'post'.");
			// 2. Validate Reason not empty → InvalidOperationException if empty
			var Reason = request.Reason?.Trim();
			if (string.IsNullOrEmpty(Reason))	throw new InvalidOperationException("Reason cannot be empty.");
			// 3. Create Report { Id = Guid.NewGuid(), ReporterId, TargetType, TargetId, Reason, Status = "pending", CreatedAt = DateTime.UtcNow }
			var report = new Domain.Entities.Report
			{
				Id = Guid.NewGuid(),
				ReporterId = reporterId,
				TargetType = TargetType,
				TargetId = request.TargetId,
				Reason = Reason,
				Status = "pending",
				CreatedAt = DateTime.UtcNow
			};
			// 4. Add, SaveChangesAsync, return ReportDto
			var entry = _appDbContext.Reports.Add(report);
			await _appDbContext.SaveChangesAsync();
			if (TargetType == "user" && request.TargetId != reporterId)
				BackgroundJob.Enqueue<INotificationService>(s =>
					s.CreateReportNotificationAsync(request.TargetId, reporterId));
			

			return new ReportDto
			{
				Id = entry.Entity.Id,
				ReporterId = entry.Entity.ReporterId,
				TargetType = entry.Entity.TargetType,
				TargetId = entry.Entity.TargetId,
				Reason = entry.Entity.Reason,
				Status = entry.Entity.Status,
				CreatedAt = entry.Entity.CreatedAt
			};
		}

		public async Task<List<ReportDto>> GetMyReportsAsync(Guid reporterId)
		{
			return await _appDbContext.Reports
						.Where(r => r.ReporterId == reporterId)
						.OrderByDescending(r => r.CreatedAt)
						.Select(r => new ReportDto
						{
							Id = r.Id,
							ReporterId = r.ReporterId,
							TargetType = r.TargetType,
							TargetId = r.TargetId,
							Reason = r.Reason,
							Status = r.Status,
							CreatedAt = r.CreatedAt
						})
						.ToListAsync();  // ← outside Select, chained after
		}

		

		


		
	}
}
