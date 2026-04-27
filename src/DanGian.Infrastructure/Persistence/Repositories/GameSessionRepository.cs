using DanGian.Domain.Enums;
using DanGian.Domain.Game;
using DanGian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence.Repositories;

internal sealed class GameSessionRepository : BaseRepository<GameSession>, IGameSessionRepository
{
    public GameSessionRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await base.GetByIdAsync(id, ct);

    public new async Task AddAsync(GameSession session, CancellationToken ct = default) =>
        await base.AddAsync(session, ct);

    public new void Update(GameSession session) =>
        base.Update(session);

    public async Task<IReadOnlyList<GameSession>> GetByPlayerIdAsync(
        Guid playerId, int page, int pageSize, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Player1Id == playerId || s.Player2Id == playerId)
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GameSession>> GetActiveByPlayerIdAsync(
        Guid playerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s =>
                s.Status == GameStatus.Playing &&
                (s.Player1Id == playerId || s.Player2Id == playerId))
            .ToListAsync(ct);
    }
}
