using Microsoft.Extensions.Logging;
using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Extensions;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Comments;
using System.Globalization;

namespace SocialPluse.Services.Services
{
	public class CommentService : ICommentService
	{
		private readonly ICommentRepository _commentRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;
		private readonly ILogger<CommentService> _logger;

		public CommentService(
			ICommentRepository commentRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher,
			ILogger<CommentService> logger)
		{
			_commentRepository = commentRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
			_logger = logger;
		}

		public async Task<CommentDto> CreateCommentAsync(Guid authorId, Guid postId, CreateCommentRequest request)
		{
			var postAuthorId = await _commentRepository.GetPostAuthorIdAsync(postId);
			if (postAuthorId is null)
			{
				_logger.LogWarning("Attempt to comment on non-existent post with id {PostId} by user {AuthorId}", postId, authorId);
				throw new KeyNotFoundException($"Post with id {postId} not found.");
			}
			var comment = new Comment
			{
				Id = Guid.NewGuid(),
				PostId = postId,
				AuthorId = authorId,
				Text = request.Text.Sanitize(),
				CreatedAt = DateTime.UtcNow
			};

			var authorUsername = await _userRepository.GetUsernameAsync(authorId);




			using var transaction = await _commentRepository.BeginTransactionAsync();
			try
			{
				await _commentRepository.AddAsync(comment);
				await _commentRepository.SaveChangesAsync();

				if (postAuthorId != authorId)
					_jobPublisher.EnqueueCommentNotificationJob(postAuthorId.Value, authorId, postId, comment.Id);

				await transaction.CommitAsync();

				_logger.LogInformation("User {AuthorId} created comment {CommentId} on post {PostId}", authorId, comment.Id, postId);

				return comment.ToDto(authorUsername ?? "Unknown");
			}
			catch(Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Failed to create comment {CommentId} on post {PostId} by user {AuthorId}. Transaction rolled back.", comment.Id, postId, authorId);
				throw;
			}
		}

		public async Task<CommentFeedResponse> GetCommentsAsync(Guid postId, string? cursor, int limit)
		{
			var clampedLimit = Math.Clamp(limit, 1, 50);
			DateTime? cursorDate = null;

			// Parse Unix Milliseconds from the string cursor
			if (cursor != null && double.TryParse(cursor, NumberStyles.Any, CultureInfo.InvariantCulture, out double ms))
			{
				cursorDate = DateTimeOffset.FromUnixTimeMilliseconds((long)ms).UtcDateTime;
			}

			var comments = await _commentRepository.GetCommentsAsync(postId, cursorDate, clampedLimit);

			var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
			var authors = await _userRepository.GetUsernamesAsync(authorIds);

			return new CommentFeedResponse
			{
				Comments = comments.Select(c => c.ToDto(authors.GetValueOrDefault(c.AuthorId, "Unknown"))).ToList(),
				// Return NextCursor as a Unix Milliseconds string
				NextCursor = comments.Count == clampedLimit
							? ((DateTimeOffset)comments.Last().CreatedAt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
							: null
			};
		}
	}
}