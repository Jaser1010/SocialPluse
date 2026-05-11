using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class SafetyRepository : ISafetyRepository
	{
		private readonly AppDbContext _context;

		public SafetyRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId) => await _context.Blocks.FindAsync(blockerId, blockedId);
		public async Task AddBlockAsync(Block block) => await _context.Blocks.AddAsync(block);
		public void RemoveBlock(Block block) => _context.Blocks.Remove(block);

		public async Task<Mute?> GetMuteAsync(Guid muterId, Guid mutedId) => await _context.Mutes.FindAsync(muterId, mutedId);
		public async Task AddMuteAsync(Mute mute) => await _context.Mutes.AddAsync(mute);
		public void RemoveMute(Mute mute) => _context.Mutes.Remove(mute);

		public async Task AddReportAsync(Report report) => await _context.Reports.AddAsync(report);

		public async Task<List<Report>> GetMyReportsAsync(Guid reporterId)
		{
			return await _context.Reports
				.AsNoTracking()
				.Where(r => r.ReporterId == reporterId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
		}

		public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

		public async Task<IDbContextTransaction> BeginTransactionAsync() =>
						await _context.Database.BeginTransactionAsync();
	}
}