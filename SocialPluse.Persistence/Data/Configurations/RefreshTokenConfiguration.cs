using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> b)
		{
			b.HasKey(x => x.Id);

			b.Property(x => x.Token)
				.IsRequired()
				.HasMaxLength(200);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			b.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => new { x.UserId, x.ExpiresAt });
		}
	}
}
