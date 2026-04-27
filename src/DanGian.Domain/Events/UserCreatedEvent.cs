namespace DanGian.Domain.Events;

public sealed record UserCreatedEvent(
    Guid UserId,
    string ZaloId,
    string DisplayName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
