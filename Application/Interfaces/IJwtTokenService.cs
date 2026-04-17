using Domain;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
