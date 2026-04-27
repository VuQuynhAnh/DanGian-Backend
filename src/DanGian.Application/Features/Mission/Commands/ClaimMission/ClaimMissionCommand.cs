using DanGian.Application.Abstractions.Messaging;

namespace DanGian.Application.Features.Mission.Commands.ClaimMission;

public sealed record ClaimMissionCommand(
    Guid UserId,
    Guid ProgressId) : ICommand<ClaimMissionResponse>;
