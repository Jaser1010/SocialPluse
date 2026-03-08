using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocialPluse.Persistence.DbContexts
{
	public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
			
		}
		override protected void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}
		public DbSet<Post> Posts { get; set; }
		public DbSet<Follow> Follows { get; set; }
		public DbSet<Like> Likes { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}
