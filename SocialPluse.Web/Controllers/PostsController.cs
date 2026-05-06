using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Web.Extensions; // Using your new User.GetUserId() extension

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Route("api")]
	public class PostsController : ControllerBase
	{
		private readonly IPostService _postService;

		public PostsController(IPostService postService) => _postService = postService;

		[Authorize]
		[HttpPost("posts")]
		public async Task<IActionResult> CreatePost(CreatePostRequest request)
		{
			// The middleware handles the 404 if the author is missing
			return Ok(await _postService.CreatePostAsync(User.GetUserId(), request));
		}

		[HttpGet("posts/{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			// Optional user ID for 'IsLiked' status
			Guid? userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
			return Ok(await _postService.GetByIdAsync(id, userId));
		}

		[Authorize]
		[HttpDelete("posts/{id:guid}")]
		public async Task<IActionResult> DeletePost(Guid id)
		{
			// The middleware[cite: 17] handles 404 (Not Found) or 403 (Forbidden)
			await _postService.DeletePostAsync(id, User.GetUserId());
			return NoContent();
		}

		[Authorize]
		[HttpGet("feed")]
		public async Task<IActionResult> GetFeed([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			return Ok(await _postService.GetFeedFromCacheAsync(User.GetUserId(), cursor, limit));
		}

		[Authorize]
		[HttpGet("feed/new")]
		public async Task<IActionResult> GetNewPostsCount([FromQuery] string? since)
		{
			if (!DateTime.TryParse(since, null, System.Globalization.DateTimeStyles.RoundtripKind, out var sinceDate))
				sinceDate = DateTime.UtcNow.AddMinutes(-1);

			var count = await _postService.GetNewPostsCountAsync(User.GetUserId(), sinceDate);
			return Ok(new { count });
		}

		[Authorize]
		[HttpGet("posts/trending")]
		public async Task<IActionResult> GetTrending([FromQuery] int limit = 8)
		{
			return Ok(await _postService.GetTrendingTopicsAsync(User.GetUserId(), limit));
		}

		[Authorize]
		[HttpPost("posts/{postId:guid}/bookmarks")]
		public async Task<IActionResult> Bookmark(Guid postId)
		{
			await _postService.ToggleBookmarkAsync(User.GetUserId(), postId, shouldBookmark: true);
			return NoContent();
		}

		[Authorize]
		[HttpDelete("posts/{postId:guid}/bookmarks")]
		public async Task<IActionResult> Unbookmark(Guid postId)
		{
			await _postService.ToggleBookmarkAsync(User.GetUserId(), postId, shouldBookmark: false);
			return NoContent();
		}

		[Authorize]
		[HttpGet("bookmarks")]
		public async Task<IActionResult> GetBookmarks([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			return Ok(await _postService.GetBookmarkedPostsAsync(User.GetUserId(), cursor, limit));
		}

		[Authorize]
		[HttpGet("analytics/me")]
		public async Task<IActionResult> GetMyAnalytics()
		{
			return Ok(await _postService.GetUserAnalyticsAsync(User.GetUserId()));
		}
	}
}