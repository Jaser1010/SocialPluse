using Hangfire;
using Microsoft.AspNetCore.Identity;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Likes;

namespace SocialPluse.Services
{
	public class LikeService : ILikeService
	{
		private readonly AppDbContext _appDbContext;
		private readonly UserManager<AppUser> _userManager;

		public LikeService(AppDbContext appDbContext, UserManager<AppUser> userManager)
		{
			_appDbContext = appDbContext;
			_userManager = userManager;
		}
		public async Task<LikeResponse> LikePostAsync(Guid userId, Guid postId)
		{
			// 1. Check post exists
			var post = await _appDbContext.Posts.FindAsync(postId);
			if (post is null)	throw new KeyNotFoundException($"Post with id {postId} not found.");
			// 2. Check already liked 
			var like = await _appDbContext.Likes.FindAsync(userId, postId);
			if (like is not null)	throw new InvalidOperationException($"User with id {userId} already liked post with id {postId}.");
			// 3. Create and save
			var newLike = new Domain.Entities.Like
			{
				UserId = userId,
				PostId = postId,
				CreatedAt = DateTime.UtcNow
			};
			var entry = await _appDbContext.Likes.AddAsync(newLike);
			await _appDbContext.SaveChangesAsync();
			if (post.AuthorId != userId)
				BackgroundJob.Enqueue<INotificationService>(s =>
					s.CreateLikeNotificationAsync(post.AuthorId, userId, postId));
			// 4. Return LikeResponse
			return new LikeResponse
			{
				UserId = entry.Entity.UserId,
				PostId = entry.Entity.PostId,
				CreatedAt = entry.Entity.CreatedAt
			};
		}

		public async Task UnlikePostAsync(Guid userId, Guid postId)
		{
			// 1. Find like 
			var like = await _appDbContext.Likes.FindAsync(userId, postId);
			if (like is null)	throw new KeyNotFoundException($"Like by user with id {userId} on post with id {postId} not found.");
			// 2. Remove and save
			var entry = _appDbContext.Likes.Remove(like);
			await _appDbContext.SaveChangesAsync();
		}
	}
}
