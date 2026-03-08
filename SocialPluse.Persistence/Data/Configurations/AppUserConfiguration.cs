using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	internal class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
	{
		public void Configure(EntityTypeBuilder<AppUser> b)
		{
			b.Property(x => x.DisplayName).HasMaxLength(50);
			b.Property(x => x.Bio).HasMaxLength(160);
			b.Property(x => x.AvatarUrl).HasMaxLength(500);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())"); // Postgres-friendly default
		}
	}
}
