namespace DanGian.Application.Features.Mission.Commands.ClaimMission;

public sealed record ClaimMissionResponse(
    int RewardPoints,
    int NewTotalPoints);
