using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Likes;
using SocialPluse.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace SocialPluse.Services.Services
{
	public class LikeService : ILikeService
	{
		private readonly ILikeRepository _likeRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;
		private readonly ILogger<LikeService> _logger;

		public LikeService(ILikeRepository likeRepository, IBackgroundJobPublisher jobPublisher, ILogger<LikeService> logger)
		{
			_likeRepository = likeRepository;
			_jobPublisher = jobPublisher;
			_logger = logger;
		}

		public async Task<LikeResponse> LikePostAsync(Guid userId, Guid postId)
		{
			var postAuthorId = await _likeRepository.GetPostAuthorIdAsync(postId);
			if (postAuthorId is null)
			{
				throw new KeyNotFoundException($"Post with id {postId} not found.");
			}
			var like = await _likeRepository.GetLikeAsync(userId, postId);
			if (like is not null) throw new InvalidOperationException($"User with id {userId} already liked post with id {postId}.");

			var newLike = new Like
			{
				UserId = userId,
				PostId = postId,
				CreatedAt = DateTime.UtcNow
			};



			using var transaction = await _likeRepository.BeginTransactionAsync();
			try
			{
				await _likeRepository.AddAsync(newLike);
				await _likeRepository.SaveChangesAsync();

				if (postAuthorId != userId)
					_jobPublisher.EnqueueLikeNotificationJob(postAuthorId.Value, userId, postId);

				await transaction.CommitAsync();
				
				_logger.LogInformation("User {UserId} liked post {PostId}.", userId, postId);	

				return newLike.ToResponse();
			}
			catch(Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error occurred while liking post {PostId} by user {UserId}.", postId, userId);
				throw;
			}
		}

		public async Task UnlikePostAsync(Guid userId, Guid postId)
		{
			var like = await _likeRepository.GetLikeAsync(userId, postId);
			if (like is null)
			{
				throw new KeyNotFoundException($"Like by user with id {userId} on post with id {postId} not found.");
			}

			_likeRepository.Remove(like);

			_logger.LogInformation("User {UserId} unliked post {PostId}.", userId, postId);

			await _likeRepository.SaveChangesAsync();
		}
	}
}