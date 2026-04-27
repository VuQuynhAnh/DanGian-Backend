using DanGian.Domain.Mission;
using DanGian.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence.Repositories;

internal sealed class MissionRepository : BaseRepository<MissionDefinition>, IMissionRepository
{
    private readonly ApplicationDbContext _context;

    public MissionRepository(ApplicationDbContext context) : base(context) =>
        _context = context;

    public async Task<MissionDefinition?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default) =>
        await base.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<MissionDefinition>> GetActiveDefinitionsAsync(CancellationToken ct = default) =>
        await DbSet.Where(m => m.IsActive).ToListAsync(ct);

    public async Task<UserMissionProgress?> GetProgressAsync(
        Guid userId, Guid definitionId, DateOnly date, CancellationToken ct = default) =>
        await _context.UserMissionProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.DefinitionId == definitionId &&
                p.Date == date, ct);

    public async Task<IReadOnlyList<UserMissionProgress>> GetUserProgressForDateAsync(
        Guid userId, DateOnly date, CancellationToken ct = default) =>
        await _context.UserMissionProgresses
            .Where(p => p.UserId == userId && p.Date == date)
            .ToListAsync(ct);

    public async Task AddProgressAsync(UserMissionProgress progress, CancellationToken ct = default) =>
        await _context.UserMissionProgresses.AddAsync(progress, ct);

    public void UpdateProgress(UserMissionProgress progress) =>
        _context.UserMissionProgresses.Update(progress);

    public async Task AddTransactionAsync(PointTransaction transaction, CancellationToken ct = default) =>
        await _context.PointTransactions.AddAsync(transaction, ct);
}
