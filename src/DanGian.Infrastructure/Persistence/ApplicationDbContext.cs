using DanGian.Domain.Game;
using DanGian.Domain.Identity;
using DanGian.Domain.Leaderboard;
using DanGian.Domain.Mission;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomPlayer> RoomPlayers => Set<RoomPlayer>();
    public DbSet<MissionDefinition> MissionDefinitions => Set<MissionDefinition>();
    public DbSet<UserMissionProgress> UserMissionProgresses => Set<UserMissionProgress>();
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<SeasonRanking> SeasonRankings => Set<SeasonRanking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
