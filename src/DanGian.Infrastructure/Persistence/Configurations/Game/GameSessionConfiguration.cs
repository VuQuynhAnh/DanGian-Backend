using DanGian.Domain.Enums;
using DanGian.Domain.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Game;

internal sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("sessions", "game");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.GameType)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<GameType>(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Mode)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<GameMode>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<GameStatus>(v, true))
            .HasMaxLength(20)
            .HasDefaultValue(GameStatus.Playing);

        builder.Property(s => s.AiDifficulty)
            .HasConversion(
                v => v == null ? null : v.ToString()!.ToLowerInvariant(),
                v => v == null ? (AiDifficulty?)null : Enum.Parse<AiDifficulty>(v, true))
            .HasMaxLength(20);

        builder.Property(s => s.InitialState)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(s => s.FinalState)
            .HasColumnType("jsonb");

        builder.Property(s => s.Moves)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'");

        builder.Property(s => s.Player1Side)
            .HasDefaultValue(1);

        builder.Property(s => s.PointsAwarded)
            .HasDefaultValue(0);

        builder.Property(s => s.StartedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(s => s.Player1Id).HasDatabaseName("idx_sessions_player1_id");
        builder.HasIndex(s => s.Player2Id).HasDatabaseName("idx_sessions_player2_id");
        builder.HasIndex(s => s.Status).HasDatabaseName("idx_sessions_status");
        builder.HasIndex(s => s.StartedAt).HasDatabaseName("idx_sessions_started_at");

        builder.Ignore(s => s.DomainEvents);
    }
}
