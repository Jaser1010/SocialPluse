using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Web.Extensions;

namespace SocialPluse.Web.Controllers
{
	[ApiController]
	[Authorize]
	public class BookmarksController : ControllerBase
	{
		private readonly IBookmarkService _bookmarkService;

		public BookmarksController(IBookmarkService bookmarkService)
		{
			_bookmarkService = bookmarkService;
		}

		// POST: api/posts/{postId}/bookmark
		[HttpPost("api/posts/{postId:guid}/bookmark")]
		public async Task<IActionResult> ToggleBookmark(Guid postId, [FromQuery] bool shouldBookmark = true)
		{
			var result = await _bookmarkService.ToggleBookmarkAsync(User.GetUserId(), postId, shouldBookmark);
			return Ok(new { Bookmarked = result });
		}

		// GET: api/bookmarks
		[HttpGet("api/bookmarks")]
		public async Task<IActionResult> GetBookmarks([FromQuery] string? cursor, [FromQuery] int limit = 20)
		{
			var result = await _bookmarkService.GetBookmarkedPostsAsync(User.GetUserId(), cursor, limit);
			return Ok(result);
		}
	}
}