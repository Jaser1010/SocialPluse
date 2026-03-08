using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
	{
		public void Configure(EntityTypeBuilder<Notification> b)
		{
			b.HasKey(x => x.Id);

			b.Property(x => x.Type).IsRequired();
			b.Property(x => x.IsRead).HasDefaultValue(false);

			b.Property(x => x.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			// Recipient FK
			b.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(x => x.RecipientUserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Actor FK
			b.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(x => x.ActorUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Optional Post FK
			b.HasOne<Post>()
				.WithMany()
				.HasForeignKey(x => x.PostId)
				.OnDelete(DeleteBehavior.SetNull);

			// Optional Comment FK
			b.HasOne<Comment>()
				.WithMany()
				.HasForeignKey(x => x.CommentId)
				.OnDelete(DeleteBehavior.SetNull);

			b.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAt });
		}
	}
}
