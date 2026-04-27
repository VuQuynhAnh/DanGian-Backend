using DanGian.Domain.Common;
using DanGian.Domain.Enums;
using DanGian.Domain.Game.ValueObjects;

namespace DanGian.Domain.Game;

public sealed class Room : AggregateRoot
{
    private readonly List<RoomPlayer> _players = [];

    private Room() { }

    private Room(Guid id, RoomCode roomCode, GameType gameType, Guid hostId) : base(id)
    {
        RoomCode = roomCode;
        GameType = gameType;
        HostId = hostId;
        ExpiresAt = DateTime.UtcNow.AddHours(2);
    }

    public RoomCode RoomCode { get; private set; } = default!;
    public GameType GameType { get; private set; }
    public Guid HostId { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Waiting;
    public int MaxPlayers { get; private set; } = 2;
    public DateTime ExpiresAt { get; private set; }
    public IReadOnlyList<RoomPlayer> Players => _players.AsReadOnly();

    public static Room Create(GameType gameType, Guid hostId)
    {
        var room = new Room(Guid.NewGuid(), RoomCode.Generate(), gameType, hostId);
        room._players.Add(RoomPlayer.Create(room.Id, hostId));
        return room;
    }

    public void Join(Guid userId)
    {
        if (Status != RoomStatus.Waiting)
            throw new InvalidOperationException("Room is not accepting players.");

        if (_players.Count >= MaxPlayers)
            throw new InvalidOperationException("Room is full.");

        if (_players.Any(p => p.UserId == userId))
            throw new InvalidOperationException("User is already in this room.");

        _players.Add(RoomPlayer.Create(Id, userId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Leave(Guid userId)
    {
        var player = _players.FirstOrDefault(p => p.UserId == userId)
            ?? throw new InvalidOperationException("User is not in this room.");

        _players.Remove(player);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReady(Guid userId, bool isReady)
    {
        var player = _players.FirstOrDefault(p => p.UserId == userId)
            ?? throw new InvalidOperationException("User is not in this room.");

        player.SetReady(isReady);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (_players.Count < 2 || !_players.All(p => p.IsReady))
            throw new InvalidOperationException("Not all players are ready.");

        Status = RoomStatus.InGame;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
