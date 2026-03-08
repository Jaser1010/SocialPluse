using SocialPluse.Persistence;
using SocialPluse.Services;
using SocialPulse.Web.Extensions;
using Scalar.AspNetCore;

namespace SocialPulse
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddPersistence(builder.Configuration);
			builder.Services.AddServices();
			builder.Services.AddControllers();
			builder.Services.AddOpenApi();
			

			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				await app.ApplyMigrationsAsync();
				app.MapOpenApi();
				app.MapScalarApiReference();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}