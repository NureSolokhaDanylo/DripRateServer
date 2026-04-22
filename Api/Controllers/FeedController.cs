using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class FeedController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public FeedController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("global")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobal([FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var query = new GetGlobalFeedQuery(_currentUser.UserId.Value, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptions([FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var query = new GetSubscriptionFeedQuery(_currentUser.UserId.Value, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("user/{username}")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserFeed(string username, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        var query = new GetUserFeedQuery(username, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
