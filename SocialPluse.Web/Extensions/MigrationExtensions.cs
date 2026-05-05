using Microsoft.EntityFrameworkCore;
using SocialPluse.Persistence.DbContexts;

namespace SocialPluse.Web.Extensions
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

			// Dev safety: ensure bookmark table exists when migration tooling is unavailable.
			await db.Database.ExecuteSqlRawAsync(@"
				CREATE TABLE IF NOT EXISTS ""Bookmarks"" (
					""UserId"" uuid NOT NULL,
					""PostId"" uuid NOT NULL,
					""CreatedAt"" timestamp with time zone NOT NULL DEFAULT timezone('utc', now()),
					CONSTRAINT ""PK_Bookmarks"" PRIMARY KEY (""UserId"", ""PostId""),
					CONSTRAINT ""FK_Bookmarks_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE,
					CONSTRAINT ""FK_Bookmarks_Posts_PostId"" FOREIGN KEY (""PostId"") REFERENCES ""Posts"" (""Id"") ON DELETE CASCADE
				);
				CREATE INDEX IF NOT EXISTS ""IX_Bookmarks_PostId"" ON ""Bookmarks"" (""PostId"");
			");
		}
	}
}
