using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Posts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Presentation.Controllers
{
	[ApiController]
	[Route("api")]
	public class PostsController : ControllerBase
	{
		private readonly IPostService _postService;

		public PostsController(IPostService postService)
		{
			_postService = postService;
		}



		// Helper — reuse in every action that needs userId
		private Guid? GetUserId()
		{
			var claim = User.FindFirst("sub")?.Value;
			return Guid.TryParse(claim, out var id) ? id : null;
		}




		[Authorize]
		[HttpPost("posts")]
		public async Task<IActionResult> CreatePost(CreatePostRequest request)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				var result = await _postService.CreatePostAsync(userId.Value, request);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}


		[HttpGet("posts/{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			try
			{
				var result = await _postService.GetByIdAsync(id);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}


		[Authorize]
		[HttpDelete("posts/{id:guid}")]
		public async Task<IActionResult> DeletePost(Guid id)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				await _postService.DeletePostAsync(id, userId.Value);
				return NoContent();
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (UnauthorizedAccessException ex)
			{
				return StatusCode(403, new { message = ex.Message });
			}
		}


		[Authorize]
		[HttpGet("feed")]
		public async Task<IActionResult> GetFeed([FromQuery] FeedRequest request)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();
			try
			{
				var result = await _postService.GetFeedAsync(userId.Value, request);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}
	}
}
