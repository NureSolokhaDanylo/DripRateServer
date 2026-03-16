using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers;

public sealed class RegisterCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public async Task<AuthResponse> Handle(RegisterCommand command)
    {
        var user = new IdentityUser
        {
            UserName = command.Username,
            Email = command.Email
        };

        var result = await _userManager.CreateAsync(user, command.Password!);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration failed: {errors}"
            };
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            AccessToken = token
        };
    }
}
