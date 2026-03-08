using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;

namespace SocialPulse.Web.Extensions
{
	public static class MigrationExtensions
	{
		public static async Task ApplyMigrationsAsync(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var pending = await db.Database.GetPendingMigrationsAsync();
			if (pending.Any())
			{
				Console.WriteLine($"Applying {pending.Count()} pending migrations...");
				await db.Database.MigrateAsync();
			}
			else
			{
				Console.WriteLine("No pending migrations.");
			}
		}
	}
}
