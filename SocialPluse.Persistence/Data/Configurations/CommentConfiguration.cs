using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class CommentConfiguration : IEntityTypeConfiguration<Comment>
	{
		public void Configure(EntityTypeBuilder<Comment> b)
		{
			b.HasKey(x => x.Id);

			b.HasOne<AppUser>()
			 .WithMany()
			 .HasForeignKey(x => x.AuthorId)
			 .OnDelete(DeleteBehavior.Restrict);

			b.HasOne<Post>()
			 .WithMany()
			 .HasForeignKey(x => x.PostId)
			 .OnDelete(DeleteBehavior.Cascade);

			b.Property(x => x.Text)
				.IsRequired()
				.HasMaxLength(2000);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			b.HasIndex(x => new { x.PostId, x.CreatedAt });
		}
	}
}
