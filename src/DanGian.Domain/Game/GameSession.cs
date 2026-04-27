using DanGian.Domain.Common;
using DanGian.Domain.Enums;
using DanGian.Domain.Events;

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
        var session = new GameSession(
            Guid.NewGuid(), gameType, mode, player1Id,
            player2Id, aiDifficulty, initialState, roomId);

        session.RaiseDomainEvent(new GameSessionCreatedEvent(
            session.Id, player1Id, player2Id, gameType, mode));

        return session;
    }

    public void Finish(
        Guid? winnerId,
        bool isDraw,
        int player1Score,
        int player2Score,
        string finalState,
        int pointsAwarded)
    {
        if (Status != GameStatus.Playing)
            throw new InvalidOperationException("Cannot finish a session that is not in playing state.");

        WinnerId = winnerId;
        IsDraw = isDraw;
        Player1Score = player1Score;
        Player2Score = player2Score;
        FinalState = finalState;
        PointsAwarded = pointsAwarded;
        Status = GameStatus.Finished;
        EndedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new GameSessionEndedEvent(
            Id, winnerId, isDraw, player1Score, player2Score, pointsAwarded));
    }

    public void Abandon()
    {
        if (Status != GameStatus.Playing)
            throw new InvalidOperationException("Cannot abandon a session that is not in playing state.");

        Status = GameStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordMove(string movesJson)
    {
        Moves = movesJson;
        UpdatedAt = DateTime.UtcNow;
    }
}
