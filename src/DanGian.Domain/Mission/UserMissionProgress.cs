using DanGian.Domain.Common;
using DanGian.Domain.Enums;
using DanGian.Domain.Events;

namespace DanGian.Domain.Mission;

public sealed class UserMissionProgress : AggregateRoot
{
    private UserMissionProgress() { }

    private UserMissionProgress(Guid id, Guid userId, Guid definitionId, int target) : base(id)
    {
        UserId = userId;
        DefinitionId = definitionId;
        Date = DateOnly.FromDateTime(DateTime.UtcNow);
        _target = target;
    }

    private readonly int _target;

    public Guid UserId { get; private set; }
    public Guid DefinitionId { get; private set; }
    public DateOnly Date { get; private set; }
    public int Progress { get; private set; }
    public MissionStatus Status { get; private set; } = MissionStatus.Pending;
    public DateTime? ClaimedAt { get; private set; }

    public static UserMissionProgress Create(Guid userId, Guid definitionId, int target)
    {
        return new UserMissionProgress(Guid.NewGuid(), userId, definitionId, target);
    }

    public void Increment(int amount = 1)
    {
        if (Status != MissionStatus.Pending) return;

        Progress = Math.Min(Progress + amount, _target);

        if (Progress >= _target)
        {
            Status = MissionStatus.Completed;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Claim(int rewardPoints)
    {
        if (Status != MissionStatus.Completed)
            throw new InvalidOperationException("Mission must be completed before claiming.");

        Status = MissionStatus.Claimed;
        ClaimedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new MissionClaimedEvent(Id, UserId, DefinitionId, rewardPoints));
    }
}
