using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;
using SocialPluse.Persistence;
using SocialPluse.Services;
using SocialPluse.Services.Abstraction;
using SocialPluse.Web.Extensions;
using SocialPluse.Web.Hubs;
using SocialPluse.Web.Middleware;
using Hangfire.Dashboard;


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
			builder.Services.AddSignalR(); // enables SignalR for real-time web functionality
			builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>(); // DI binding for notification sender using SignalR
			builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>(); // JWT claim mapping for SignalR user identification

			// CORS policy for React frontend
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", policy =>
					policy.WithOrigins("http://localhost:5173")
						  .AllowAnyHeader()
						  .AllowAnyMethod()
						  .AllowCredentials());
			});



			var app = builder.Build(); // Build the application




			app.UseCors("AllowFrontend"); // Enable CORS for React frontend
			app.UseGlobalExceptionMiddleware();  // Add global exception handling middleware
			if (!app.Environment.IsDevelopment())
				app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
			app.UseAuthentication(); // Enable authentication middleware
			app.UseAuthorization(); // Enable authorization middleware


			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = []  // allow all — dev only!
			});

			if (app.Environment.IsDevelopment()) // Only apply migrations and map OpenAPI in development environment
			{
				await app.ApplyMigrationsAsync(); // Apply database migrations at startup
				app.MapOpenApi(); // Map OpenAPI/Swagger endpoints for API documentation
				app.MapScalarApiReference(); // Map Scalar API reference for development/testing purposes
				// app.UseHangfireDashboard("/hangfire"); // dev monitoring UI
			}




			app.MapHub<NotificationHub>("/hubs/notifications"); // registers the WebSocket endpoint
			app.MapControllers(); // Map controller routes



			app.Run(); // Run the application
		}
	}
}