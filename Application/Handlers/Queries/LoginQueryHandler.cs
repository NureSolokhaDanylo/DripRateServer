using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Queries;

public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, ErrorOr<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginQueryHandler(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ErrorOr<AuthResponse>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(query.Username);
        if (user == null)
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid username or password");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, query.Password);
        if (!passwordValid)
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid username or password");
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(token, user.Id, user.UserName!, user.Email!);
    }
}
