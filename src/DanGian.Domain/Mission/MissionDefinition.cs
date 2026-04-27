using DanGian.Domain.Common;
using DanGian.Domain.Enums;

namespace DanGian.Domain.Mission;

public sealed class MissionDefinition : AggregateRoot
{
    private MissionDefinition() { }

    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public int RewardPoints { get; private set; }
    public int Target { get; private set; }
    public ResetType ResetType { get; private set; }
    public string? GameType { get; private set; }
    public bool IsActive { get; private set; } = true;
}
