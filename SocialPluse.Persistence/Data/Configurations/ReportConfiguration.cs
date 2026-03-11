using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;

namespace SocialPluse.Persistence.Data.Configurations
{
	public class ReportConfiguration : IEntityTypeConfiguration<Report>
	{
		public void Configure(EntityTypeBuilder<Report> builder)
		{
			builder.HasKey(r => r.Id);

			builder.Property(r => r.TargetType)
				.IsRequired()
				.HasMaxLength(10);

			builder.Property(r => r.Reason)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(r => r.Status)
				.IsRequired()
				.HasMaxLength(20)
				.HasDefaultValue("pending");

			builder.Property(r => r.CreatedAt)
				.HasDefaultValueSql("timezone('utc', now())");

			builder.HasIndex(r => r.ReporterId);
			builder.HasIndex(r => new { r.TargetType, r.TargetId });

			builder.HasOne<AppUser>()
				.WithMany()
				.HasForeignKey(r => r.ReporterId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
