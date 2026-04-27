namespace DanGian.Application.Features.Identity.Queries.GetProfile;

public sealed record GetProfileResponse(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    string RankTitle,
    int TotalPoints,
    DateTime? LastLoginAt);
