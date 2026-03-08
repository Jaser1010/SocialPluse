using SocialPluse.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface IAuthService
	{
		public Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
		public Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
	}
}
