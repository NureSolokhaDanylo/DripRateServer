using Api.Attributes;
using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiErrors(StatusCodes.Status400BadRequest, AuthErrors.EmailAlreadyTakenCode, AuthErrors.UserNameAlreadyTakenCode)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Use request.Username ?? string.Empty if we want to handle nulls, 
        // but RegisterCommand expects string.
        var command = new RegisterCommand(request.Username ?? string.Empty, request.Email, request.Password);

        var result = await _mediator.Send(command);

        return result.Match(
            id => Ok(id),
            errors => Problem(errors));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ApiErrors(StatusCodes.Status401Unauthorized, AuthErrors.InvalidCredentialsCode)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.Username, request.Password);

        var result = await _mediator.Send(query);

        return result.Match(
            token => Ok(token),
            errors => Problem(errors));
    }

    [Authorize]
    [HttpDelete("account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount([FromServices] ICurrentUser currentUser)
    {
        if (currentUser.UserId is null)
        {
            return Unauthorized();
        }

        var command = new DeleteUserCommand(currentUser.UserId.Value);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
