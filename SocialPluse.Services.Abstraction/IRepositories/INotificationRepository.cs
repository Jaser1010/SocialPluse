using SocialPluse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPluse.Services.Abstraction.IRepositories
{
	public interface INotificationRepository
	{
		Task AddAsync(Notification notification);
		Task<List<Notification>> GetNotificationsAsync(Guid userId, DateTime? cursor, int limit);
		Task<Notification?> GetByIdAsync(Guid notificationId);
		Task<int> SaveChangesAsync();
	}
}