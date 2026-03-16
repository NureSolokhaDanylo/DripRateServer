using Application.Commands;
using Application.Dtos;
using Application.Queries;
using Application.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;
    private readonly LoginQueryHandler _loginHandler;

    public AuthController(
        RegisterCommandHandler registerHandler,
        LoginQueryHandler loginHandler)
    {
        _registerHandler = registerHandler ?? throw new ArgumentNullException(nameof(registerHandler));
        _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        var result = await _registerHandler.Handle(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _loginHandler.Handle(query);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
