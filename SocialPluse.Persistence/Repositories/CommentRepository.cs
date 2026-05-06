using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IRepositories;

namespace SocialPluse.Persistence.Repositories
{
	public class CommentRepository : ICommentRepository
	{
		private readonly AppDbContext _context;

		public CommentRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> PostExistsAsync(Guid postId)
		{
			return await _context.Posts.AnyAsync(p => p.Id == postId);
		}

		public async Task<Guid?> GetPostAuthorIdAsync(Guid postId)
		{
			var post = await _context.Posts
				.Where(p => p.Id == postId)
				.Select(p => p.AuthorId)
				.FirstOrDefaultAsync();

			return post == Guid.Empty ? null : post;
		}

		public async Task AddAsync(Comment comment)
		{
			await _context.Comments.AddAsync(comment);
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<List<Comment>> GetCommentsAsync(Guid postId, DateTime? cursor, int limit)
		{
			var query = _context.Comments.Where(c => c.PostId == postId);

			if (cursor.HasValue)
			{
				query = query.Where(c => c.CreatedAt < cursor.Value);
			}

			return await query
				.OrderByDescending(c => c.CreatedAt)
				.Take(limit)
				.ToListAsync();
		}
	}
}