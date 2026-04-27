using DanGian.Domain.Identity;

namespace DanGian.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<bool> ExistsByZaloIdAsync(string zaloId, CancellationToken ct = default);
}
