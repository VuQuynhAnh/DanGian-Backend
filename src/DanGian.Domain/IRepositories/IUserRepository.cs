using DanGian.Domain.Identity;

namespace DanGian.Domain.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<bool> ExistsByZaloIdAsync(string zaloId, CancellationToken ct = default);
}
