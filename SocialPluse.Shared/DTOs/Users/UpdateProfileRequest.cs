using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Users
{
	public class UpdateProfileRequest
	{
		public string? DisplayName { get; set; }
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }
	}
}
