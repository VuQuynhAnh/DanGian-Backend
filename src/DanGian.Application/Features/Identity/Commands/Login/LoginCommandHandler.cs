using DanGian.Application.Abstractions;
using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Events;
using DanGian.Domain.Identity;
using DanGian.Domain.IRepositories;
using DanGian.Domain.Primitives;
using MediatR;

namespace DanGian.Application.Features.Identity.Commands.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPublisher _publisher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IPublisher publisher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _publisher = publisher;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByZaloIdAsync(request.ZaloId, cancellationToken);

        bool isNewUser = user is null;

        if (isNewUser)
        {
            user = User.Create(request.ZaloId, request.DisplayName, request.AvatarUrl);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            user!.UpdateProfile(request.DisplayName, request.AvatarUrl);
            _userRepository.Update(user);
        }

        user!.RecordLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (isNewUser)
            await _publisher.Publish(
                new UserCreatedEvent(user.Id, user.ZaloId, user.DisplayName),
                cancellationToken);

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
