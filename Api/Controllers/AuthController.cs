using Api.Attributes;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class AuthController : ApiController
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiErrors(StatusCodes.Status400BadRequest, "Auth.DuplicateEmail", "Auth.DuplicateUsername")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password);

        var result = await _mediator.Send(command);

        return result.Match(
            authResponse => Ok(authResponse),
            errors => Problem(errors));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ApiErrors(StatusCodes.Status401Unauthorized, "Auth.InvalidCredentials")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.Username, request.Password);

        var result = await _mediator.Send(query);

        return result.Match(
            authResponse => Ok(authResponse),
            errors => Problem(errors));
    }
}
