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
    [ApiErrors(AuthErrors.EmailAlreadyTakenCode)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Use request.DisplayName ?? string.Empty if we want to handle nulls, 
        // but RegisterCommand expects string.
        var command = new RegisterCommand(request.DisplayName ?? string.Empty, request.Email, request.Password);

        var result = await _mediator.Send(command);

        return result.Match(
            id => Ok(id),
            errors => Problem(errors));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ApiErrors(AuthErrors.InvalidCredentialsCode)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.Email, request.Password);

        var result = await _mediator.Send(query);

        return result.Match(
            token => Ok(token),
            errors => Problem(errors));
    }

    [AuthorizeWithError]
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(UserErrors.NotFoundCode)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] ICurrentUser currentUser)
    {
        var command = new ChangePasswordCommand(currentUser.UserId.Value, request.OldPassword, request.NewPassword);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [AuthorizeWithError]
    [HttpDelete("account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(UserErrors.NotFoundCode, UserErrors.DeleteFailedCode)]
    public async Task<IActionResult> DeleteAccount([FromServices] ICurrentUser currentUser)
    {
        var command = new DeleteUserCommand(currentUser.UserId.Value);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
