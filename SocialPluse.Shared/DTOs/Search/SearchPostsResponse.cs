using SocialPluse.Shared.DTOs.Posts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Search
{
	public class SearchPostsResponse
	{
		public List<PostDto> Posts { get; set; } = [];
	}
}
