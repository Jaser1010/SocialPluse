using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Services.Abstraction;
using SocialPluse.Shared.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialPluse.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IConfiguration _configuration;

		public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_configuration = configuration;
		}
		public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
		{
			// 1. Find user by email 
			var user = await _userManager.FindByEmailAsync(loginRequest.Email);
			if (user == null)
				throw new UnauthorizedAccessException("Invalid credentials.");
			// 2. Check password
			var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
			if (!passwordValid)
				throw new UnauthorizedAccessException("Invalid credentials.");
			// 3. return new AuthResponse { ... }
			return new AuthResponse
			{
				AccessToken = GenerateJwt(user),
				Username = user.UserName!,
				Email = user.Email!
			};
		}

		public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
		{
			// 1. Check if username is taken 
			var existingUser = await _userManager.FindByNameAsync(registerRequest.Username);
			if (existingUser != null)
				throw new InvalidOperationException("Username is already taken.");
			// 2. Create a new AppUser with UserName, Email, CreatedAt = DateTime.UtcNow
			var user = new AppUser()
			{
				UserName = registerRequest.Username,
				Email = registerRequest.Email,
				CreatedAt = DateTime.UtcNow
			};
			// 3. Call _userManager.CreateAsync(user, request.Password)
			var result = await _userManager.CreateAsync(user, registerRequest.Password);
			if (!result.Succeeded)
				throw new InvalidOperationException(
				string.Join(", ", result.Errors.Select(e => e.Description)));
			// 4. return new AuthResponse{ ... };
			return new AuthResponse
			{
				AccessToken = GenerateJwt(user),
				Username = user.UserName!,
				Email = user.Email!
			};
		}



		private string GenerateJwt(AppUser user)
		{
			// STEP 1 — WHO is this token for?
			// Claims are key-value pairs baked into the token payload
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),       // subject = userId
				new Claim(JwtRegisteredClaimNames.Email, user.Email!),            // email
			 new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),    // username
			 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique token id
			};
			// STEP 2 — Read settings from appsettings.json
			var key = _configuration["Jwt:Key"]!; // the secret used to sign
			var issuer = _configuration["Jwt:Issuer"]!; // who created the token
			var audience = _configuration["Jwt:Audience"]!; // who is the token for
			var expiry = int.Parse(_configuration["Jwt:ExpiryMinutes"]!); // / how long it lives
			// STEP 3 — Create the signing key from your secret string
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

			// STEP 4 — Define HOW it will be signed
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			// STEP 5 — Build the actual token object
			var token = new JwtSecurityToken(
					issuer: issuer,
					audience: audience,
					claims: claims,
					expires: DateTime.UtcNow.AddMinutes(expiry),
					signingCredentials: credentials
					);

			// STEP 6 — Serialize it to the eyJ... string
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
