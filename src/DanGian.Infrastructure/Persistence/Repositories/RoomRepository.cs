using DanGian.Domain.Game;
using DanGian.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence.Repositories;

internal sealed class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Room?> GetByCodeAsync(string roomCode, CancellationToken ct = default) =>
        await DbSet
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.RoomCode.Value == roomCode, ct);

    public new async Task AddAsync(Room room, CancellationToken ct = default) =>
        await base.AddAsync(room, ct);

    public new void Update(Room room) =>
        base.Update(room);
}
