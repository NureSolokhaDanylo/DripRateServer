using Application.Commands;
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
}
