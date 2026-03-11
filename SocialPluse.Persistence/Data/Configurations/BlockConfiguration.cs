using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;


namespace SocialPluse.Persistence.Data.Configurations
{
	public class BlockConfiguration : IEntityTypeConfiguration<Block>
	{
		public void Configure(EntityTypeBuilder<Block> builder)
		{
			// Composite PK
			builder.HasKey(b => new { b.BlockerId, b.BlockedId });

			// UTC default
			builder.Property(b => b.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			// Self-block check
			builder.ToTable(t => t.HasCheckConstraint(
				"CK_Block_NotSelf",
				"\"BlockerId\" != \"BlockedId\""));

			// Delete behavior — restrict both sides to avoid cascade loops
			builder.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(b => b.BlockerId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(b => b.BlockedId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
