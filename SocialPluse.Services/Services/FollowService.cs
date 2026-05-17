using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Follows;
using SocialPluse.Services.Mappers;
using Microsoft.Extensions.Logging;


namespace SocialPluse.Services.Services
{
	public class FollowService : IFollowService
	{
		private readonly IFollowRepository _followRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;
		private readonly ILogger<FollowService> _logger;

		public FollowService(
			IFollowRepository followRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher,
			ILogger<FollowService> logger)
		{
			_followRepository = followRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
			_logger = logger;
		}

		public async Task<FollowResponse> FollowAsync(Guid followerId, Guid followeeId)
		{
			if (followerId == followeeId)
				throw new InvalidOperationException("You cannot follow yourself.");

			// Fast database check
			var userExists = await _userRepository.UserExistsAsync(followeeId);
			if (!userExists)
			{
				_logger.LogWarning("Attempt to follow non-existent user {FolloweeId} by user {FollowerId}.", followeeId, followerId);
				throw new KeyNotFoundException("User not found.");
			}
			var existing = await _followRepository.GetFollowAsync(followerId, followeeId);
			if (existing != null)
			{
				_logger.LogWarning("Attempt to follow already followed user {FolloweeId} by user {FollowerId}.", followeeId, followerId);
				throw new InvalidOperationException("You are already following this user.");
			}

			var follow = new Follow
			{
				FollowerId = followerId,
				FolloweeId = followeeId,
				CreatedAt = DateTime.UtcNow
			};

			using var transaction = await _followRepository.BeginTransactionAsync();


			try
			{
				await _followRepository.AddAsync(follow);
				await _followRepository.SaveChangesAsync();

				// Side effects are now logically part of the atomic operation
				_jobPublisher.EnqueueFollowNotificationJob(followeeId, followerId);
				_jobPublisher.EnqueueBackfillFeedJob(followerId, followeeId);

				await transaction.CommitAsync();

				_logger.LogInformation("User {FollowerId} followed user {FolloweeId}.", followerId, followeeId);

				return follow.ToResponse();
			}
			catch
			{
				await transaction.RollbackAsync();
				_logger.LogError("Failed to follow user {FolloweeId} by user {FollowerId}. Transaction rolled back.", followeeId, followerId);
				throw; // The GlobalExceptionMiddleware will handle this
			}
		}

		public async Task UnfollowAsync(Guid followerId, Guid followeeId)
		{
			var follow = await _followRepository.GetFollowAsync(followerId, followeeId);
			if (follow == null)
			{
				_logger.LogWarning("Attempt to unfollow user {FolloweeId} by user {FollowerId} which was not followed.", followeeId, followerId);
				throw new KeyNotFoundException("You are not following this user.");
			}

			_followRepository.Remove(follow);
			await _followRepository.SaveChangesAsync();

			_jobPublisher.EnqueueInvalidateFeedCacheJob(followerId);
			_logger.LogInformation("User {FollowerId} unfollowed user {FolloweeId}.", followerId, followeeId);
		}

		public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
		{
			return await _followRepository.IsFollowingAsync(followerId, followeeId);
		}
	}
}