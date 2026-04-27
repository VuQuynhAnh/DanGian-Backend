namespace DanGian.Application.Features.Leaderboard.Queries.GetLeaderboard;

public sealed record LeaderboardResponse(
    int SeasonNum,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<LeaderboardEntry> Entries);

public sealed record LeaderboardEntry(
    int Rank,
    Guid UserId,
    int Points);
