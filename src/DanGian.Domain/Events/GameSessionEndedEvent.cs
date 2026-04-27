namespace DanGian.Domain.Events;

public sealed record GameSessionEndedEvent(
    Guid SessionId,
    Guid? WinnerId,
    bool IsDraw,
    int Player1Score,
    int Player2Score,
    int PointsAwarded) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
