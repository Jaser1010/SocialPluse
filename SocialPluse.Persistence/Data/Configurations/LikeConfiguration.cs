using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class LikeConfiguration : IEntityTypeConfiguration<Like>
	{
		public void Configure(EntityTypeBuilder<Like> b)
		{
			b.HasKey(x => new { x.UserId, x.PostId });

			b.HasOne<AppUser>()
			 .WithMany()
			 .HasForeignKey(x => x.UserId)
			 .OnDelete(DeleteBehavior.Cascade);

			b.HasOne<Post>()
			 .WithMany()
			 .HasForeignKey(x => x.PostId)
			 .OnDelete(DeleteBehavior.Cascade);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			b.HasIndex(x => x.PostId);
		}
	}
}
