using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Follows;


namespace SocialPluse.Services
{
	public class FollowService : IFollowService
	{
		private readonly AppDbContext _appDbContext;
		private readonly UserManager<AppUser> _userManager;

		public FollowService(AppDbContext appDbContext, UserManager<AppUser> userManager)
		{
			_appDbContext = appDbContext;
			_userManager = userManager;
		}
		public async Task<FollowResponse> FollowAsync(Guid followerId, Guid followeeId)
		{
			// 1. Check followerId == followeeId → throw InvalidOperationException("You cannot follow yourself.")
			if(followerId == followeeId) throw new InvalidOperationException("You cannot follow yourself.");
			// 2. Check followee exists via UserManager → throw KeyNotFoundException if not
			var followee = await _userManager.FindByIdAsync(followeeId.ToString());
			if (followee == null) throw new KeyNotFoundException("User not found.");
			// 3. Check already following
			var existing = await _appDbContext.Follows.FindAsync(followerId, followeeId);
			if (existing != null)
				throw new InvalidOperationException("You are already following this user.");
			// 4. Create and save
			var follow = new Follow
			{
				FollowerId = followerId,
				FolloweeId = followeeId,
				CreatedAt = DateTime.UtcNow
			};

			_appDbContext.Follows.Add(follow);
			await _appDbContext.SaveChangesAsync();
			BackgroundJob.Enqueue<INotificationService>(s =>
									s.CreateFollowNotificationAsync(followeeId, followerId));

			return new FollowResponse
			{
				FollowerId = followerId,
				FolloweeId = followeeId,
				CreatedAt = follow.CreatedAt
			};
		}

		public async Task UnfollowAsync(Guid followerId, Guid followeeId)
		{
			// 1. Find the follow record → if null throw KeyNotFoundException
			var follow = await _appDbContext.Follows.FindAsync(followerId, followeeId);
			if (follow == null)
				throw new KeyNotFoundException("You are not following this user.");
			// 2. Remove(follow)
			_appDbContext.Follows.Remove(follow);
			// 3. SaveChangesAsync()	
			await _appDbContext.SaveChangesAsync();
		}
	}
}
