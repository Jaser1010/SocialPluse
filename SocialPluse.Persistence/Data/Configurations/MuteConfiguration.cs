using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;


namespace SocialPluse.Persistence.Data.Configurations
{
	public class MuteConfiguration : IEntityTypeConfiguration<Mute>
	{
		public void Configure(EntityTypeBuilder<Mute> builder)
		{
			builder.HasKey(m => new { m.MuterId, m.MutedId });

			builder.Property(m => m.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			builder.ToTable(t => t.HasCheckConstraint(
				"CK_Mute_NotSelf",
				"\"MuterId\" != \"MutedId\""));

			builder.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(m => m.MuterId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(m => m.MutedId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
