using DanGian.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.ZaloId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasColumnType("text");

        builder.Property(u => u.TotalPoints)
            .HasDefaultValue(0);

        builder.Property(u => u.RankTitle)
            .HasMaxLength(50)
            .HasDefaultValue("Thôn");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(u => u.ZaloId)
            .IsUnique()
            .HasDatabaseName("idx_users_zalo_id");

        builder.HasIndex(u => u.TotalPoints)
            .HasDatabaseName("idx_users_total_points");

        builder.Ignore(u => u.DomainEvents);
    }
}
