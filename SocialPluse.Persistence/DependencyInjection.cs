using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
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

			return services;
		}
	}
}
