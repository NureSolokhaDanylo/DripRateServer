using Application.Dtos;
using Domain;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, IList<string> roles);
    AuthResponse GenerateAuthResponse(User user, IList<string> roles);
}
