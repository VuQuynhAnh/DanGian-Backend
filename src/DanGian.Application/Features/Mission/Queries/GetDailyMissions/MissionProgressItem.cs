using DanGian.Domain.Enums;

namespace DanGian.Application.Features.Mission.Queries.GetDailyMissions;

public sealed record MissionProgressItem(
    Guid ProgressId,
    Guid DefinitionId,
    string Title,
    string? Description,
    int Progress,
    int Target,
    int RewardPoints,
    MissionStatus Status);
