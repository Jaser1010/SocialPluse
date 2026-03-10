using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Posts
{
	public class CreatePostRequest
	{
		public string Text { get; set; } = default!;
		public string? MediaUrl { get; set; }
	}
}
