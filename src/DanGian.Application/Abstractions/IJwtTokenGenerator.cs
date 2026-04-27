using DanGian.Domain.Identity;

namespace DanGian.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
