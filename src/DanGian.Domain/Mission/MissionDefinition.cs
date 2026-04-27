using DanGian.Domain.Common;
using DanGian.Domain.Enums;

namespace DanGian.Domain.Mission;

public sealed class MissionDefinition : AggregateRoot
{
    private MissionDefinition() { }

    private MissionDefinition(
        Guid id,
        string type,
        string title,
        string? description,
        int rewardPoints,
        int target,
        ResetType resetType,
        string? gameType) : base(id)
    {
        Type = type;
        Title = title;
        Description = description;
        RewardPoints = rewardPoints;
        Target = target;
        ResetType = resetType;
        GameType = gameType;
    }

    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public int RewardPoints { get; private set; }
    public int Target { get; private set; }
    public ResetType ResetType { get; private set; }
    public string? GameType { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static MissionDefinition Create(
        string type,
        string title,
        string? description,
        int rewardPoints,
        int target,
        ResetType resetType,
        string? gameType = null)
    {
        return new MissionDefinition(
            Guid.NewGuid(), type, title, description,
            rewardPoints, target, resetType, gameType);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
