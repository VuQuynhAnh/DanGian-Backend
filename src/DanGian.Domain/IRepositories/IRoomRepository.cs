using DanGian.Domain.Game;

namespace DanGian.Domain.IRepositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Room?> GetByCodeAsync(string roomCode, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    void Update(Room room);
}
