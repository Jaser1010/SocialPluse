using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class FollowConfiguration : IEntityTypeConfiguration<Follow>
	{
		public void Configure(EntityTypeBuilder<Follow> b)
		{
			b.HasKey(x => new { x.FollowerId, x.FolloweeId });

			b.HasOne<AppUser>()
			 .WithMany()
			 .HasForeignKey(x => x.FollowerId)
			 .OnDelete(DeleteBehavior.Restrict);

			b.HasOne<AppUser>()
			 .WithMany()
			 .HasForeignKey(x => x.FolloweeId)
			 .OnDelete(DeleteBehavior.Restrict);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			b.HasIndex(x => x.FollowerId);
			b.HasIndex(x => x.FolloweeId);

			// Optional safety rule at DB level (Postgres check constraint)
			b.ToTable(t => t.HasCheckConstraint("CK_Follow_NotSelf", "\"FollowerId\" <> \"FolloweeId\""));
		}
	}
}
