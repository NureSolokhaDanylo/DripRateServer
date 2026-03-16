using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers;

public sealed class LoginQueryHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginQueryHandler(
        UserManager<IdentityUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public async Task<AuthResponse> Handle(LoginQuery query)
    {
        var user = await _userManager.FindByNameAsync(query.Username!);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, query.Password!);
        if (!passwordValid)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = token
        };
    }
}
