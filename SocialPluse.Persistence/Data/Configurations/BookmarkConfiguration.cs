using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
	{
		public void Configure(EntityTypeBuilder<Bookmark> b)
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
