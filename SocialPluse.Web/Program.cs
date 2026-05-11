using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;
using SocialPluse.Persistence;
using SocialPluse.Persistence.BackgroundJobs;
using SocialPluse.Services;
using SocialPluse.Services.Abstraction.IService;
using SocialPluse.Shared.Validators;
using SocialPluse.Web.Extensions;
using SocialPluse.Web.Hubs;
using SocialPluse.Web.Middleware;

namespace SocialPluse
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// --- 1. Service Registrations ---
			builder.Services.AddPersistence(builder.Configuration);
			builder.Services.AddServices();
			builder.Services.AddControllers();
			builder.Services.AddOpenApi();
			builder.Services.AddSignalR();

			builder.Services.AddFluentValidationAutoValidation();
			builder.Services.AddValidatorsFromAssemblyContaining<CreatePostRequestValidator>();

			// Required for building Media URLs in LocalMediaService
			builder.Services.AddHttpContextAccessor();

			builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>();
			builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", policy =>
					policy.WithOrigins("http://localhost:5173")
						  .AllowAnyHeader()
						  .AllowAnyMethod()
						  .AllowCredentials());
			});

			// Start the Outbox Engine here!
			builder.Services.AddHostedService<OutboxProcessor>();

			var app = builder.Build();

			// --- 2. Middleware Pipeline (Order is Critical) ---

			// ABSOLUTE TOP: Must catch errors from all other middlewares
			app.UseMiddleware<RequestLogMiddleware>();
			app.UseGlobalExceptionMiddleware();

			app.UseCors("AllowFrontend");
			app.UseStaticFiles();

			if (!app.Environment.IsDevelopment())
				app.UseHttpsRedirection();

			// Identity must come before Authorization
			app.UseAuthentication();
			app.UseAuthorization();

			// --- 3. Feature Endpoints ---
			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = [] // Use proper authorization in production!
			});

			if (app.Environment.IsDevelopment())
			{
				await app.ApplyMigrationsAsync();
				app.MapOpenApi();
				app.MapScalarApiReference();
			}

			app.MapHub<NotificationHub>("/hubs/notifications");
			app.MapControllers();

			app.Run();
		}
	}
}