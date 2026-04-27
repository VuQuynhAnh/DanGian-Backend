using DanGian.Domain.Leaderboard;
using DanGian.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence.Repositories;

internal sealed class LeaderboardRepository : BaseRepository<Season>, ILeaderboardRepository
{
    private readonly ApplicationDbContext _context;

    public LeaderboardRepository(ApplicationDbContext context) : base(context) =>
        _context = context;

    public async Task<Season?> GetActiveSeasonAsync(CancellationToken ct = default) =>
        await DbSet
            .Include(s => s.Rankings)
            .FirstOrDefaultAsync(s => s.IsActive, ct);

    public new async Task<Season?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(s => s.Rankings)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddSeasonAsync(Season season, CancellationToken ct = default) =>
        await base.AddAsync(season, ct);

    public void UpdateSeason(Season season) =>
        base.Update(season);

    public async Task<IReadOnlyList<SeasonRanking>> GetTopRankingsAsync(
        Guid seasonId, int count, CancellationToken ct = default) =>
        await _context.SeasonRankings
            .Where(r => r.SeasonId == seasonId)
            .OrderBy(r => r.Rank)
            .Take(count)
            .ToListAsync(ct);

    public async Task<SeasonRanking?> GetUserRankingAsync(
        Guid seasonId, Guid userId, CancellationToken ct = default) =>
        await _context.SeasonRankings
            .FirstOrDefaultAsync(r => r.SeasonId == seasonId && r.UserId == userId, ct);

    public async Task AddRankingAsync(SeasonRanking ranking, CancellationToken ct = default) =>
        await _context.SeasonRankings.AddAsync(ranking, ct);
}
