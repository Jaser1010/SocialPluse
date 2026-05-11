using SocialPluse.Domain.Entities;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Services.Extensions;
using SocialPluse.Services.Mappers;
using SocialPluse.Shared.DTOs.Posts;

namespace SocialPluse.Services
{
	public class PostService : IPostService
	{
		private readonly IPostRepository _postRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBackgroundJobPublisher _jobPublisher;

		public PostService(
			IPostRepository postRepository,
			IUserRepository userRepository,
			IBackgroundJobPublisher jobPublisher)
		{
			_postRepository = postRepository;
			_userRepository = userRepository;
			_jobPublisher = jobPublisher;
		}

		public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostRequest request)
		{
			var username = await _userRepository.GetUsernameAsync(authorId);
			if (username == null) throw new KeyNotFoundException($"User with ID {authorId} not found.");

			var post = new Post
			{
				Id = Guid.NewGuid(),
				AuthorId = authorId,
				Text = request.Text.Sanitize(), // Hardened input to prevent XSS
				MediaUrl = request.MediaUrl,
				CreatedAt = DateTime.UtcNow
			};

			// Senior Fix: Atomic Write for Database & Hangfire
			using var transaction = await _postRepository.BeginTransactionAsync();
			try
			{
				await _postRepository.AddAsync(post);
				await _postRepository.SaveChangesAsync();

				// Fanout job remains here because creating a post TRIGGERS the feed update
				_jobPublisher.EnqueuePostFanoutJob(post.Id, post.AuthorId);

				await transaction.CommitAsync();
				return post.ToDto(username, 0, 0, false, false);
			}
			catch
			{
				await transaction.RollbackAsync();
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
			if (post == null) throw new KeyNotFoundException("Post not found.");

			if (post.AuthorId != requestingUserId)
				throw new UnauthorizedAccessException("You can only delete your own posts.");

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
		}
	}
}