using DanGian.Domain.Enums;
using DanGian.Domain.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Game;

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms", "game");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomCode)
            .HasConversion(
                v => v.Value,
                v => Domain.Game.ValueObjects.RoomCode.Create(v))
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(r => r.GameType)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<GameType>(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<RoomStatus>(v, true))
            .HasMaxLength(20)
            .HasDefaultValue(RoomStatus.Waiting);

        builder.Property(r => r.MaxPlayers)
            .HasDefaultValue(2);

        builder.Property(r => r.ExpiresAt)
            .HasDefaultValueSql("NOW() + INTERVAL '2 hours'");

        builder.HasMany(r => r.Players)
            .WithOne()
            .HasForeignKey(rp => rp.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.RoomCode)
            .IsUnique()
            .HasDatabaseName("idx_rooms_room_code");

        builder.Ignore(r => r.DomainEvents);
    }
}
