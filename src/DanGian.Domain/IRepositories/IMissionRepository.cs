using DanGian.Domain.Mission;

namespace DanGian.Domain.IRepositories;

public interface IMissionRepository
{
    Task<MissionDefinition?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MissionDefinition>> GetActiveDefinitionsAsync(CancellationToken ct = default);

    Task<UserMissionProgress?> GetProgressAsync(
        Guid userId, Guid definitionId, DateOnly date, CancellationToken ct = default);

    Task<IReadOnlyList<UserMissionProgress>> GetUserProgressForDateAsync(
        Guid userId, DateOnly date, CancellationToken ct = default);

    Task AddProgressAsync(UserMissionProgress progress, CancellationToken ct = default);
    void UpdateProgress(UserMissionProgress progress);

    Task AddTransactionAsync(PointTransaction transaction, CancellationToken ct = default);
}
