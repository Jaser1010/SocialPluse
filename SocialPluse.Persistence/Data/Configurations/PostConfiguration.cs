using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence
{
	public class PostConfiguration : IEntityTypeConfiguration<Post>
	{
		public void Configure(EntityTypeBuilder<Post> b)
		{
			b.HasKey(x => x.Id);

			b.Property(x => x.Text)
				.IsRequired()
				.HasMaxLength(2000);

			b.Property(x => x.MediaUrl)
				.HasMaxLength(1000);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			// Feed/profile index
			b.HasIndex(x => new { x.AuthorId, x.CreatedAt });

			// FK to Identity user
			b.HasOne<AppUser>()
			 .WithMany()
			 .HasForeignKey(x => x.AuthorId)
			 .OnDelete(DeleteBehavior.Cascade);


			b.Property(p => p.SearchVector).HasComputedColumnSql(
								"to_tsvector('english', coalesce(\"Text\", ''))",stored: true);

			b.HasIndex(p => p.SearchVector).HasMethod("GIN");
		}
	}
}
