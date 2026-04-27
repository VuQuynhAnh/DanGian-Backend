using DanGian.Application.Abstractions;
using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Game;
using DanGian.Domain.Primitives;
using DanGian.Domain.Repositories;

namespace DanGian.Application.Features.Game.Commands.CreateSession;

internal sealed class CreateSessionCommandHandler
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSessionCommandHandler(
        IGameSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateSessionResponse>> Handle(
        CreateSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = GameSession.Create(
            request.GameType,
            request.Mode,
            request.Player1Id,
            request.InitialState,
            request.Player2Id,
            request.AiDifficulty);

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateSessionResponse(
            session.Id,
            session.GameType,
            session.Mode,
            session.StartedAt));
    }
}
