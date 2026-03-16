using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(IdentityUser user);
}
