using Application.Commands;
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
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class GamesController : ApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public GamesController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet("first-impression")]
    public async Task<IActionResult> GetFirstImpressionBatch([FromQuery] int batchSize = 10, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var query = new GetFirstImpressionBatchQuery(_currentUser.UserId.Value, batchSize);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            games => Ok(games),
            Problem
        );
    }

    [HttpPost("first-impression")]
    public async Task<IActionResult> SubmitFirstImpressionBatch([FromBody] SubmitFirstImpressionBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var userCommand = command with { UserId = _currentUser.UserId.Value };
        var result = await _sender.Send(userCommand, cancellationToken);

        return result.Match(
            _ => Ok(),
            Problem
        );
    }

    [HttpGet("guess-price")]
    public async Task<IActionResult> GetGuessPriceBatch([FromQuery] int batchSize = 10, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var query = new GetGuessPriceBatchQuery(_currentUser.UserId.Value, batchSize);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            games => Ok(games),
            Problem
        );
    }

    [HttpPost("guess-price")]
    public async Task<IActionResult> SubmitGuessPriceBatch([FromBody] SubmitGuessPriceBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var userCommand = command with { UserId = _currentUser.UserId.Value };
        var result = await _sender.Send(userCommand, cancellationToken);

        return result.Match(
            _ => Ok(),
            Problem
        );
    }

    [HttpGet("tag-match")]
    public async Task<IActionResult> GetTagMatchBatch([FromQuery] int batchSize = 10, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var query = new GetTagMatchBatchQuery(_currentUser.UserId.Value, batchSize);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            games => Ok(games),
            Problem
        );
    }

    [HttpPost("tag-match")]
    public async Task<IActionResult> SubmitTagMatchBatch([FromBody] SubmitTagMatchBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        var userCommand = command with { UserId = _currentUser.UserId.Value };
        var result = await _sender.Send(userCommand, cancellationToken);

        return result.Match(
            _ => Ok(),
            Problem
        );
    }
}
