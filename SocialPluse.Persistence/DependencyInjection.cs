using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Persistence.Repositories;
using SocialPluse.Persistence.Services;
using SocialPluse.Services.Abstraction.IRepositories;
using SocialPluse.Services.Abstraction.IService;
using StackExchange.Redis;
using System.Text;

namespace SocialPluse.Persistence
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
		{


			services.AddScoped<IMediaService, LocalMediaService>();
			services.AddScoped<ISearchRepository, SearchRepository>();
			services.AddScoped<ISafetyRepository, SafetyRepository>();
			services.AddScoped<INotificationRepository, NotificationRepository>();
			services.AddScoped<ILikeRepository, LikeRepository>();
			services.AddScoped<IFeedCacheService, RedisFeedCacheService>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IPostRepository, PostRepository>();
			services.AddScoped<IBackgroundJobPublisher, HangfireJobPublisher>();


			services.AddScoped<IIdentityWrapper, IdentityWrapper>();




			// Database configuration
			services.AddDbContext<AppDbContext>(options =>
				options.UseNpgsql(config.GetConnectionString("Postgres")));

			// Identity configuration
			services.AddIdentityCore<AppUser>(options =>
			{
				options.User.RequireUniqueEmail = true;
				options.Password.RequiredLength = 8;
			})
			.AddRoles<IdentityRole<Guid>>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddSignInManager();

			// JWT Authentication configuration
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

				// Add event handlers for debugging purposes
				options.Events = new JwtBearerEvents
				{
					OnTokenValidated = context => Task.CompletedTask,
					OnAuthenticationFailed = context => Task.CompletedTask
				};
			});

			// Hangfire configuration
			services.AddHangfire(hangfire => hangfire.UsePostgreSqlStorage(c =>
									c.UseNpgsqlConnection(config.GetConnectionString("Postgres"))));




			// Redis cache configuration
			services.AddStackExchangeRedisCache(options =>
									options.Configuration = config.GetConnectionString("Redis"));
			services.AddSingleton<IConnectionMultiplexer>(
				ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));




			// Add Hangfire server to process background jobs
			services.AddHangfireServer();

			// Add authorization services (if needed for policies or role-based access)
			services.AddAuthorization();

			return services;
		}
	}
}