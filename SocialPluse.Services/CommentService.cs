using Microsoft.Extensions.Hosting;
using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Comments;
using System.Globalization;

namespace SocialPluse.Services
{
	public class CommentService : ICommentService
	{
		private readonly ICommentRepository _commentRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;

		public CommentService(
			ICommentRepository commentRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher)
		{
			_commentRepository = commentRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
		}

		public async Task<CommentDto> CreateCommentAsync(Guid authorId, Guid postId, CreateCommentRequest request)
		{
			var postAuthorId = await _commentRepository.GetPostAuthorIdAsync(postId);
			if (postAuthorId is null) throw new KeyNotFoundException($"Post with id {postId} not found.");

			var comment = new Comment
			{
				Id = Guid.NewGuid(),
				PostId = postId,
				AuthorId = authorId,
				Text = request.Text,
				CreatedAt = DateTime.UtcNow
			};

			var authorUsername = await _userRepository.GetUsernameAsync(authorId);

			await _commentRepository.AddAsync(comment);
			await _commentRepository.SaveChangesAsync();

			if (postAuthorId != authorId)
			{
				_jobPublisher.EnqueueCommentNotificationJob(postAuthorId.Value, authorId, postId, comment.Id);
			}

			return comment.ToDto(authorUsername ?? "Unknown");
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