using Api.Attributes;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Application.Commands.Advertisements;
using Application.Queries.Advertisements;
using Application.Commands.Auth;
using Application.Queries.Auth;
using Application.Commands.Collections;
using Application.Queries.Collections;
using Application.Queries.Feed;
using Application.Commands.Games;
using Application.Queries.Games;
using Application.Queries.Moderation;
using Application.Commands.Publications;
using Application.Queries.Publications;
using Application.Commands.Assessments;
using Application.Queries.Assessments;
using Application.Commands.Reports;
using Application.Queries.Search;
using Application.Commands.Social;
using Application.Queries.Social;
using Application.Queries.Tags;
using Application.Commands.Users;
using Application.Queries.Users;
using Application.Commands.Wardrobe;
using Application.Queries.Wardrobe;
using Application.Commands.Comments;
using Application.Queries.Comments;
using Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError]
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
    [ProducesResponseType(typeof(GlobalFeedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobal([FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
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
        var query = new GetSubscriptionFeedQuery(_currentUser.UserId.Value, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserFeed(Guid userId, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        var query = new GetUserFeedQuery(userId, _currentUser.UserId, cursor, take);
        var result = await _mediator.Send(query);
 
        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("top")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTop(
        [FromQuery] TopFeedPeriod period = TopFeedPeriod.Weekly,
        [FromQuery] bool onlyFollowing = false,
        [FromQuery] List<Guid>? tagIds = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var query = new GetTopFeedQuery(_currentUser.UserId.Value, period, onlyFollowing, tagIds, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
