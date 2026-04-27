using DanGian.Domain.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Game;

internal sealed class RoomPlayerConfiguration : IEntityTypeConfiguration<RoomPlayer>
{
    public void Configure(EntityTypeBuilder<RoomPlayer> builder)
    {
        builder.ToTable("room_players", "game");

        builder.HasKey(rp => new { rp.RoomId, rp.UserId });

        builder.Property(rp => rp.JoinedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(rp => rp.IsReady)
            .HasDefaultValue(false);
    }
}
