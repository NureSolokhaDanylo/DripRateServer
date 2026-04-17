using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Commands;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ErrorOr<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var user = new User(command.Email, command.Username);

        var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            return result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(token, user.Id, user.UserName!, user.Email!);
    }
}
