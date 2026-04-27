using DanGian.Application.Abstractions;
using DanGian.Application.Abstractions.Messaging;
using DanGian.Domain.Events;
using DanGian.Domain.Game;
using DanGian.Domain.IRepositories;
using DanGian.Domain.Primitives;
using MediatR;

namespace DanGian.Application.Features.Game.Commands.CreateSession;

internal sealed class CreateSessionCommandHandler
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CreateSessionCommandHandler(
        IGameSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
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

        await _publisher.Publish(
            new GameSessionCreatedEvent(session.Id, session.Player1Id, session.Player2Id, session.GameType, session.Mode),
            cancellationToken);

        return Result.Success(new CreateSessionResponse(
            session.Id,
            session.GameType,
            session.Mode,
            session.StartedAt));
    }
}
