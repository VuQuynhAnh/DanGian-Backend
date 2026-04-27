using DanGian.Domain.Identity;
using DanGian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DanGian.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await base.GetByIdAsync(id, ct);

    public async Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.ZaloId == zaloId && u.DeletedAt == null, ct);

    public new async Task AddAsync(User user, CancellationToken ct = default) =>
        await base.AddAsync(user, ct);

    public new void Update(User user) =>
        base.Update(user);

    public async Task<bool> ExistsByZaloIdAsync(string zaloId, CancellationToken ct = default) =>
        await DbSet.AnyAsync(u => u.ZaloId == zaloId, ct);
}
