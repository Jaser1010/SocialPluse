using SocialPluse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface ISafetyRepository
	{
		// Blocks
		Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId);
		Task AddBlockAsync(Block block);
		void RemoveBlock(Block block);

		// Mutes
		Task<Mute?> GetMuteAsync(Guid muterId, Guid mutedId);
		Task AddMuteAsync(Mute mute);
		void RemoveMute(Mute mute);

		// Reports
		Task AddReportAsync(Report report);
		Task<List<Report>> GetMyReportsAsync(Guid reporterId);

		Task<int> SaveChangesAsync();
	}
}