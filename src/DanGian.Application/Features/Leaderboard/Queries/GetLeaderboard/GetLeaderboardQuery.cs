using DanGian.Application.Abstractions.Messaging;

namespace DanGian.Application.Features.Leaderboard.Queries.GetLeaderboard;

public sealed record GetLeaderboardQuery(
    int TopCount = 100) : IQuery<LeaderboardResponse>;
