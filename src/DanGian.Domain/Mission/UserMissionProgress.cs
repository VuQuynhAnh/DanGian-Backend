using DanGian.Domain.Common;
using DanGian.Domain.Enums;

namespace DanGian.Domain.Mission;

public sealed class UserMissionProgress : AggregateRoot
{
    private UserMissionProgress() { }

    public Guid UserId { get; private set; }
    public Guid DefinitionId { get; private set; }
    public DateOnly Date { get; private set; }
    public int Progress { get; private set; }
    public MissionStatus Status { get; private set; } = MissionStatus.Pending;
    public DateTime? ClaimedAt { get; private set; }
}
