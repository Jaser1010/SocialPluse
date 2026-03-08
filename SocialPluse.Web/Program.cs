using SocialPluse.Persistence;
using SocialPulse.Web.Extensions;
namespace SocialPulse
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddPersistence(builder.Configuration);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddOpenApi();
			

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				await app.ApplyMigrationsAsync();
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}
