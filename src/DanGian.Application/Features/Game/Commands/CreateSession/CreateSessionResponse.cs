using DanGian.Domain.Enums;

namespace DanGian.Application.Features.Game.Commands.CreateSession;

public sealed record CreateSessionResponse(
    Guid SessionId,
    GameType GameType,
    GameMode Mode,
    DateTime StartedAt);
