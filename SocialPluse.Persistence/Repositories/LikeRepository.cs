using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class LikeRepository : ILikeRepository
	{
		private readonly AppDbContext _context;

		public LikeRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<Guid?> GetPostAuthorIdAsync(Guid postId)
		{
			var post = await _context.Posts
				.Where(p => p.Id == postId)
				.Select(p => p.AuthorId)
				.FirstOrDefaultAsync();

			return post == Guid.Empty ? null : post;
		}

		public async Task<Like?> GetLikeAsync(Guid userId, Guid postId)
		{
			return await _context.Likes.FindAsync(userId, postId);
		}

		public async Task AddAsync(Like like)
		{
			await _context.Likes.AddAsync(like);
		}

		public void Remove(Like like)
		{
			_context.Likes.Remove(like);
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<IDbContextTransaction> BeginTransactionAsync() =>
				await _context.Database.BeginTransactionAsync();
	}
}