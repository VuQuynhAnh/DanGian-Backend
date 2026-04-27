using DanGian.Domain.Common;

namespace DanGian.Domain.Identity;

public sealed class User : AggregateRoot
{
    private User() { }

    private User(Guid id, string zaloId, string displayName, string? avatarUrl)
        : base(id)
    {
        ZaloId = zaloId;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
    }

    public string ZaloId { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public int TotalPoints { get; private set; }
    public string RankTitle { get; private set; } = "Thôn";
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static User Create(string zaloId, string displayName, string? avatarUrl = null) =>
        new(Guid.NewGuid(), zaloId, displayName, avatarUrl);

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
