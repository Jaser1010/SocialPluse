using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Comments;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services
{
	public class CommentService : ICommentService
	{
		private readonly AppDbContext _appDbContext;
		private readonly UserManager<AppUser> _userManager;

		public CommentService(AppDbContext appDbContext, UserManager<AppUser> userManager)
		{
			_appDbContext = appDbContext;
			_userManager = userManager;
		}
		public async Task<CommentDto> CreateCommentAsync(Guid authorId, Guid postId, CreateCommentRequest request)
		{
			// 1. Check post exists → KeyNotFoundException if null
			var postExists = await _appDbContext.Posts.AnyAsync(p => p.Id == postId);
			if (!postExists)
				throw new KeyNotFoundException($"Post with id {postId} not found.");
			// 2. Create and save
			var comment = new Comment
			{
				Id = Guid.NewGuid(),
				PostId = postId,
				AuthorId = authorId,
				Text = request.Text,
				CreatedAt = DateTime.UtcNow
			};
			// 3. Get author username via UserManager
			var author = await _userManager.FindByIdAsync(authorId.ToString());


			await _appDbContext.Comments.AddAsync(comment);
			await _appDbContext.SaveChangesAsync();
			// 4. Return CommentDto
			return new CommentDto
			{
				Id = comment.Id,
				PostId = comment.PostId,
				AuthorId = comment.AuthorId,
				AuthorUsername = author?.UserName ?? "Unknown",
				Text = comment.Text,
				CreatedAt = comment.CreatedAt
			};
		}

		public async Task<CommentFeedResponse> GetCommentsAsync(Guid postId, DateTime? cursor, int limit)
		{
			// 1. Build query
			var query = _appDbContext.Comments.Where(c => c.PostId == postId);
			if (cursor.HasValue)
				query = query.Where(c => c.CreatedAt < cursor.Value);
			// 2. Clamp limit: Math.Clamp(limit, 1, 50)
			var clampedLimit = Math.Clamp(limit, 1, 50);
			// 3. Order, take, fetch usernames (same pattern as feed)
			var comments = await query.OrderByDescending(c => c.CreatedAt)
				.Take(clampedLimit)
				.ToListAsync();
			// Batch fetch usernames
			var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
			var authors = await _userManager.Users
				.Where(u => authorIds.Contains(u.Id))
				.ToDictionaryAsync(u => u.Id, u => u.UserName!);
			// 4. Return CommentFeedResponse with NextCursor
			return new CommentFeedResponse
			{
				Comments = comments.Select(c => new CommentDto
				{
					Id = c.Id,
					PostId = c.PostId,
					AuthorId = c.AuthorId,
					AuthorUsername = authors.GetValueOrDefault(c.AuthorId, "Unknown"),
					Text = c.Text,
					CreatedAt = c.CreatedAt
				}).ToList(),
				NextCursor = comments.Count == clampedLimit ? comments.Last().CreatedAt : null
			};
		}		
	}
}
