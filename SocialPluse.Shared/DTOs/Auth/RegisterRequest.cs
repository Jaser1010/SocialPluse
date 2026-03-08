using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Shared.DTOs.Auth
{
	public class RegisterRequest
	{
		public string Username { get; set; } = default!;
		public string Email { get; set; } = default!;
		public string Password { get; set; } = default!;
	}
}
