using DanGian.Domain.Enums;

namespace DanGian.Domain.Events;

public sealed record GameSessionCreatedEvent(
    Guid SessionId,
    Guid Player1Id,
    Guid? Player2Id,
    GameType GameType,
    GameMode Mode) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
