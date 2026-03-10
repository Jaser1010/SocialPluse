using Scalar.AspNetCore;
using SocialPluse.Persistence;
using SocialPluse.Services;
using SocialPluse.Web.Extensions;
using SocialPluse.Web.Middleware;

namespace SocialPluse
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddPersistence(builder.Configuration); // Add persistence services (e.g., DbContext, Identity)
			builder.Services.AddServices(); // Add application services (e.g., IUserService, IAuthService)
			builder.Services.AddControllers(); // Add controllers for API endpoints
			builder.Services.AddOpenApi(); // Add OpenAPI/Swagger services for API documentation


			var app = builder.Build(); // Build the application

			app.UseGlobalExceptionMiddleware();  // Add global exception handling middleware

			if (app.Environment.IsDevelopment()) // Only apply migrations and map OpenAPI in development environment
			{
				await app.ApplyMigrationsAsync(); // Apply database migrations at startup
				app.MapOpenApi(); // Map OpenAPI/Swagger endpoints for API documentation
				app.MapScalarApiReference(); // Map Scalar API reference for development/testing purposes
			}

			app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
			app.UseAuthentication(); // Enable authentication middleware
			app.UseAuthorization(); // Enable authorization middleware
			app.MapControllers(); // Map controller routes

			app.Run(); // Run the application
		}
	}
}