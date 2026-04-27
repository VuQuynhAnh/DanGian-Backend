using DanGian.Domain.Common;
using DanGian.Domain.Events;

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

    public static User Create(string zaloId, string displayName, string? avatarUrl = null)
    {
        var user = new User(Guid.NewGuid(), zaloId, displayName, avatarUrl);
        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, zaloId, displayName));
        return user;
    }

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPoints(int points)
    {
        if (points <= 0) return;
        TotalPoints += points;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
