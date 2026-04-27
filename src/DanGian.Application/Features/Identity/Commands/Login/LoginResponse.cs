namespace DanGian.Application.Features.Identity.Commands.Login;

public sealed record LoginResponse(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string RankTitle,
    int TotalPoints,
    string AccessToken);
