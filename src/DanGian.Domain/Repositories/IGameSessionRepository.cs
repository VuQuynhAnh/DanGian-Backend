using DanGian.Domain.Game;

namespace DanGian.Domain.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(GameSession session, CancellationToken ct = default);
    void Update(GameSession session);
    Task<IReadOnlyList<GameSession>> GetByPlayerIdAsync(
        Guid playerId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<GameSession>> GetActiveByPlayerIdAsync(
        Guid playerId, CancellationToken ct = default);
}
