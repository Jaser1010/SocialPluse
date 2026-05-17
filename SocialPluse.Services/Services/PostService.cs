using Microsoft.Extensions.Logging;
using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Extensions;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services.Services
{
	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;
		private readonly ILogger<PostService> _logger;

		public PostService(
			IPostRepository postRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher,
			ILogger<PostService> logger)
		{
			_postRepository = postRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
			_logger = logger;
		}

		public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request)
		{
			var username = await _userRepository.GetUsernameAsync(authorId);
			if (username == null) 
			{
				_logger.LogWarning("Post creation failed: User {AuthorId} not found.", authorId);
				throw new KeyNotFoundException($"User with ID {authorId} not found."); 
			}

			var post = new Post
			{
				Id = Guid.NewGuid(),
				AuthorId = authorId,
				Text = request.Text.Sanitize(), // Hardened input to prevent XSS
				MediaUrl = request.MediaUrl,
				CreatedAt = DateTime.UtcNow
			};

			// 100% Atomic Write via Outbox Pattern
			using var transaction = await _postRepository.BeginTransactionAsync();
			try
			{
				// 1. Add the post to EF tracking
				await _postRepository.AddAsync(post);

				// 2. Add the outbox message to EF tracking (Inside this method)
				_jobPublisher.EnqueuePostFanoutJob(post.Id, post.AuthorId);

				// 3. Save BOTH to the database simultaneously! 
				await _postRepository.SaveChangesAsync();

				await transaction.CommitAsync();
				// 4. Business Event Success
				_logger.LogInformation("User {UserId} created Post {PostId}", authorId, post.Id);

				return post.ToDto(username, 0, 0, false, false);
			}
			catch(Exception ex)
			{
				await transaction.RollbackAsync();
				// 5. Critical Failure Tracking (Include 'ex' to capture Stack Trace)
				_logger.LogError(ex, "Transaction rolled back while creating Post for User {UserId}", authorId);
				throw;
			}
		}

		public async Task<PostDto> GetByIdAsync(Guid postId, Guid? currentUserId = null)
		{
			var post = await _postRepository.GetByIdAsync(postId);
			if (post == null) throw new KeyNotFoundException("Post not found.");

			var username = await _userRepository.GetUsernameAsync(post.AuthorId);

			// Fetch engagement metrics so the single post view is accurate
			var likes = await _postRepository.GetLikeCountsAsync(new[] { postId });
			var comments = await _postRepository.GetCommentCountsAsync(new[] { postId });

			bool isLiked = false;
			bool isBookmarked = false;

			if (currentUserId.HasValue)
			{
				var likedPosts = await _postRepository.GetLikedPostIdsAsync(currentUserId.Value, new[] { postId });
				isLiked = likedPosts.Contains(postId);

				var bookmarkedPosts = await _postRepository.GetBookmarkedPostIdsAsync(currentUserId.Value, new[] { postId });
				isBookmarked = bookmarkedPosts.Contains(postId);
			}

			return post.ToDto(
				username ?? "Unknown",
				likes.GetValueOrDefault(postId, 0),
				comments.GetValueOrDefault(postId, 0),
				isLiked,
				isBookmarked);
		}

		public async Task DeletePostAsync(Guid postId, Guid requestingUserId)
		{
			var post = await _postRepository.GetByIdAsync(postId);
			if (post == null)
			{
				_logger.LogWarning("Post deletion failed: Post {PostId} not found.", postId); 
				throw new KeyNotFoundException("Post not found.");
			}
			if (post.AuthorId != requestingUserId)
			{
				// 6. SECURITY EVENT: A user tried to delete someone else's post! 
				// We MUST log both the attacker and the victim IDs.
				_logger.LogWarning("Security Alert: User {RequestingUserId} attempted to delete Post {PostId} owned by User {OwnerId}",
					requestingUserId, postId, post.AuthorId);

				throw new UnauthorizedAccessException("You can only delete your own posts.");
			}
			using var transaction = await _postRepository.BeginTransactionAsync();
			try
			{
				await _postRepository.DeleteAsync(post);
				await _postRepository.SaveChangesAsync();

				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}

			_logger.LogInformation("User {UserId} deleted Post {PostId}", requestingUserId, postId);
		}
	}
}