using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Enums;

namespace DanGian.Application.Features.Game.Commands.CreateSession;

public sealed record CreateSessionCommand(
    GameType GameType,
    GameMode Mode,
    Guid Player1Id,
    string InitialState,
    Guid? Player2Id = null,
    AiDifficulty? AiDifficulty = null) : ICommand<CreateSessionResponse>;
