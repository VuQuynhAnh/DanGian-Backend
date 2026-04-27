using DanGian.Application.Abstractions.Messaging;

namespace DanGian.Application.Features.Identity.Commands.Login;

public sealed record LoginCommand(
    string ZaloId,
    string DisplayName,
    string? AvatarUrl) : ICommand<LoginResponse>;
