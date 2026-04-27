namespace DanGian.Domain.Events;

public sealed record MissionClaimedEvent(
    Guid ProgressId,
    Guid UserId,
    Guid DefinitionId,
    int RewardPoints) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
