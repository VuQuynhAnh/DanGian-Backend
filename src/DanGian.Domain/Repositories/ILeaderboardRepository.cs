using DanGian.Domain.Leaderboard;

namespace DanGian.Domain.Repositories;

public interface ILeaderboardRepository
{
    Task<Season?> GetActiveSeasonAsync(CancellationToken ct = default);
    Task<Season?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddSeasonAsync(Season season, CancellationToken ct = default);
    void UpdateSeason(Season season);

    Task<IReadOnlyList<SeasonRanking>> GetTopRankingsAsync(
        Guid seasonId, int count, CancellationToken ct = default);

    Task<SeasonRanking?> GetUserRankingAsync(
        Guid seasonId, Guid userId, CancellationToken ct = default);

    Task AddRankingAsync(SeasonRanking ranking, CancellationToken ct = default);
}
