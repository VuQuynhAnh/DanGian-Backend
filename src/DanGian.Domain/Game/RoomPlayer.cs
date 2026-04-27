using DanGian.Domain.Common;

namespace DanGian.Domain.Game;

public sealed class RoomPlayer : Entity
{
    private RoomPlayer() { }

    private RoomPlayer(Guid roomId, Guid userId) : base(Guid.NewGuid())
    {
        RoomId = roomId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }

    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsReady { get; private set; }

    internal static RoomPlayer Create(Guid roomId, Guid userId) =>
        new(roomId, userId);

    internal void SetReady(bool isReady)
    {
        IsReady = isReady;
        UpdatedAt = DateTime.UtcNow;
    }
}
