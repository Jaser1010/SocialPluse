using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Auth
{
	public class AuthResponse
	{
		public string AccessToken { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Email { get; set; } = default!;
	}
}
