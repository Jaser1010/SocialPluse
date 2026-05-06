using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Follows;
using SocialPluse.Services.Mappers;

namespace SocialPluse.Services
{
	public class FollowService : IFollowService
	{
		private readonly IFollowRepository _followRepository;
		private readonly IUserRepository _userRepository; // Senior fix: using Repo instead of Service
		private readonly IBackgroundJobPublisher _jobPublisher;

		public FollowService(
			IFollowRepository followRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher)
		{
			_followRepository = followRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
		}

		public async Task<FollowResponse> FollowAsync(Guid followerId, Guid followeeId)
		{
			if (followerId == followeeId)
				throw new InvalidOperationException("You cannot follow yourself.");

			// Fast database check
			var userExists = await _userRepository.UserExistsAsync(followeeId);
			if (!userExists)
				throw new KeyNotFoundException("User not found.");

			var existing = await _followRepository.GetFollowAsync(followerId, followeeId);
			if (existing != null)
				throw new InvalidOperationException("You are already following this user.");

			var follow = new Follow
			{
				FollowerId = followerId,
				FolloweeId = followeeId,
				CreatedAt = DateTime.UtcNow
			};

			await _followRepository.AddAsync(follow);
			await _followRepository.SaveChangesAsync();

			_jobPublisher.EnqueueFollowNotificationJob(followeeId, followerId);
			_jobPublisher.EnqueueBackfillFeedJob(followerId, followeeId);

			return follow.ToResponse();
		}

		public async Task UnfollowAsync(Guid followerId, Guid followeeId)
		{
			var follow = await _followRepository.GetFollowAsync(followerId, followeeId);
			if (follow == null)
				throw new KeyNotFoundException("You are not following this user.");

			_followRepository.Remove(follow);
			await _followRepository.SaveChangesAsync();

			_jobPublisher.EnqueueInvalidateFeedCacheJob(followerId);
		}

		public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
		{
			return await _followRepository.IsFollowingAsync(followerId, followeeId);
		}
	}
}