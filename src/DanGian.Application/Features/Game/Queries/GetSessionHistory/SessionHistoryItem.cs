using DanGian.Domain.Enums;

namespace DanGian.Application.Features.Game.Queries.GetSessionHistory;

public sealed record SessionHistoryItem(
    Guid SessionId,
    GameType GameType,
    GameMode Mode,
    GameStatus Status,
    Guid? WinnerId,
    bool IsDraw,
    int PlayerScore,
    int PointsAwarded,
    DateTime StartedAt,
    DateTime? EndedAt);
