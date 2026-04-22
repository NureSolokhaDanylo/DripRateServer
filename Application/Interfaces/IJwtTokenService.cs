using Application.Dtos;
using Domain;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    AuthResponse GenerateAuthResponse(User user);
}
