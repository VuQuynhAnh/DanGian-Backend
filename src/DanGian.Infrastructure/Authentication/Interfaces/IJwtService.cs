using DanGian.Domain.Identity;

namespace DanGian.Infrastructure.Authentication;

public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}
