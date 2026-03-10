using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using System.Text;

namespace SocialPluse.Persistence
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
		{
			services.AddDbContext<AppDbContext>(options =>
				options.UseNpgsql(config.GetConnectionString("Postgres")));

			services.AddIdentityCore<AppUser>(options =>
			{
				options.User.RequireUniqueEmail = true;
				options.Password.RequiredLength = 8;
			})
			.AddRoles<IdentityRole<Guid>>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddSignInManager();

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.MapInboundClaims = false;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = config["Jwt:Issuer"],
					ValidAudience = config["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
					ClockSkew = TimeSpan.Zero
				};

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						Console.WriteLine($"Authorization Header: {context.Request.Headers.Authorization}");
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = context =>
					{
						Console.WriteLine("AUTH FAILED:");
						Console.WriteLine(context.Exception.ToString());
						return Task.CompletedTask;
					},
					OnChallenge = context =>
					{
						Console.WriteLine($"CHALLENGE ERROR: {context.Error}");
						Console.WriteLine($"CHALLENGE DESCRIPTION: {context.ErrorDescription}");
						return Task.CompletedTask;
					},
					OnTokenValidated = context =>
					{
						Console.WriteLine("TOKEN VALIDATED");
						foreach (var claim in context.Principal!.Claims)
						{
							Console.WriteLine($"{claim.Type} = {claim.Value}");
						}
						return Task.CompletedTask;
					}
				};
			});

			services.AddAuthorization();

			return services;
		}
	}
}