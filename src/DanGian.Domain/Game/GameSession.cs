using DanGian.Domain.Common;
using DanGian.Domain.Enums;

namespace DanGian.Domain.Game;

public sealed class GameSession : AggregateRoot
{
    private GameSession() { }

    private GameSession(
        Guid id,
        GameType gameType,
        GameMode mode,
        Guid player1Id,
        Guid? player2Id,
        AiDifficulty? aiDifficulty,
        string initialState,
        Guid? roomId) : base(id)
    {
        GameType = gameType;
        Mode = mode;
        Player1Id = player1Id;
        Player2Id = player2Id;
        AiDifficulty = aiDifficulty;
        InitialState = initialState;
        RoomId = roomId;
        StartedAt = DateTime.UtcNow;
    }

    public GameType GameType { get; private set; }
    public GameMode Mode { get; private set; }
    public Guid Player1Id { get; private set; }
    public Guid? Player2Id { get; private set; }
    public Guid? WinnerId { get; private set; }
    public bool IsDraw { get; private set; }
    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }
    public int Player1Side { get; private set; } = 1;
    public AiDifficulty? AiDifficulty { get; private set; }
    public string InitialState { get; private set; } = default!;
    public string? FinalState { get; private set; }
    public string Moves { get; private set; } = "[]";
    public int PointsAwarded { get; private set; }
    public GameStatus Status { get; private set; } = GameStatus.Playing;
    public Guid? RoomId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    public static GameSession Create(
        GameType gameType,
        GameMode mode,
        Guid player1Id,
        string initialState,
        Guid? player2Id = null,
        AiDifficulty? aiDifficulty = null,
        Guid? roomId = null)
    {
        return new GameSession(
            Guid.NewGuid(), gameType, mode, player1Id,
            player2Id, aiDifficulty, initialState, roomId);
    }

}
