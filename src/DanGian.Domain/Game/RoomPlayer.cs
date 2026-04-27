using DanGian.Domain.Common;

namespace DanGian.Domain.Game;

public sealed class RoomPlayer : Entity
{
    private RoomPlayer() { }

    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsReady { get; private set; }
}
