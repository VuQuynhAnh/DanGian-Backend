using DanGian.Domain.Common;
using DanGian.Domain.Enums;
using DanGian.Domain.Game.ValueObjects;

namespace DanGian.Domain.Game;

public sealed class Room : AggregateRoot
{
    private Room() { }

    public RoomCode RoomCode { get; private set; } = default!;
    public GameType GameType { get; private set; }
    public Guid HostId { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Waiting;
    public int MaxPlayers { get; private set; } = 2;
    public DateTime ExpiresAt { get; private set; }
}
