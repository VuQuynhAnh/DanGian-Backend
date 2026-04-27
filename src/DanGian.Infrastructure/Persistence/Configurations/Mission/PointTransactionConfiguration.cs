using DanGian.Domain.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Mission;

internal sealed class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.ToTable("point_transactions", "mission");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Reason)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("idx_point_tx_user_id");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("idx_point_tx_created_at");
    }
}
