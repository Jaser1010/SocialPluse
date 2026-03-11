using SocialPluse.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Search
{
	public class SearchUsersResponse
	{
		public List<UserProfileDto> Users { get; set; } = [];
	}
}
