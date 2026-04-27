using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Identity;
using DanGian.Domain.Primitives;
using DanGian.Domain.IRepositories;

namespace DanGian.Application.Features.Identity.Queries.GetProfile;

internal sealed class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, GetProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(IUserRepository userRepository) =>
        _userRepository = userRepository;

    public async Task<Result<GetProfileResponse>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<GetProfileResponse>(
                Error.NotFound(nameof(user), request.UserId));

        return Result.Success(new GetProfileResponse(
            user.Id,
            user.DisplayName,
            user.AvatarUrl,
            user.RankTitle,
            user.TotalPoints,
            user.LastLoginAt));
    }
}
