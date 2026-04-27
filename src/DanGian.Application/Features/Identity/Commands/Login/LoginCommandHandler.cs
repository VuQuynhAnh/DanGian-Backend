using DanGian.Application.Abstractions;
using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Identity;
using DanGian.Domain.Primitives;
using DanGian.Domain.Repositories;

namespace DanGian.Application.Features.Identity.Commands.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByZaloIdAsync(request.ZaloId, cancellationToken);

        if (user is null)
        {
            user = User.Create(request.ZaloId, request.DisplayName, request.AvatarUrl);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            user.UpdateProfile(request.DisplayName, request.AvatarUrl);
            _userRepository.Update(user);
        }

        user.RecordLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result.Success(new LoginResponse(
            user.Id,
            user.DisplayName,
            user.AvatarUrl,
            user.RankTitle,
            user.TotalPoints,
            token));
    }
}
