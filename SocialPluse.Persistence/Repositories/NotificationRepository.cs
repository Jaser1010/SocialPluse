using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly AppDbContext _context;

		public NotificationRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Notification notification)
		{
			await _context.Notifications.AddAsync(notification);
		}

		public async Task<List<Notification>> GetNotificationsAsync(Guid userId, DateTime? cursor, int limit)
		{
			var query = _context.Notifications.AsNoTracking().Where(n => n.RecipientUserId == userId);

			if (cursor.HasValue)
				query = query.Where(n => n.CreatedAt < cursor.Value);

			return await query
				.OrderByDescending(n => n.CreatedAt)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<Notification?> GetByIdAsync(Guid notificationId)
		{
			return await _context.Notifications.FindAsync(notificationId);
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}