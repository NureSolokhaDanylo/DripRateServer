using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CollectionsController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public CollectionsController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("@me")]
    [ProducesResponseType(typeof(List<CollectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCollections()
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var query = new GetMyCollectionsQuery(_currentUser.UserId.Value);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCollectionRequest request)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new CreateCollectionCommand(
            _currentUser.UserId.Value,
            request.Name,
            request.Description,
            request.IsPublic);

        var result = await _mediator.Send(command);

        return result.Match(
            id => CreatedAtAction(nameof(Create), new { id }, id),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetItems(Guid id, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var query = new GetCollectionItemsQuery(id, _currentUser.UserId.Value, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/items/{pubId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddItem(Guid id, Guid pubId)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new AddToCollectionCommand(_currentUser.UserId.Value, id, pubId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}/items/{pubId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid pubId)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new RemoveFromCollectionCommand(_currentUser.UserId.Value, id, pubId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
