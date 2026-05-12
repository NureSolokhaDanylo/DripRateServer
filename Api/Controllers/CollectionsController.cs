using Api.Attributes;
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
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError]
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
    public async Task<IActionResult> GetMyCollections([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var query = new GetMyCollectionsQuery(_currentUser.UserId.Value, skip, take);
        var result = await _mediator.Send(query);
 
        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ApiErrors(CollectionErrors.NameAlreadyExistsCode)]
    public async Task<IActionResult> Create([FromBody] CreateCollectionRequest request)
    {
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

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(CollectionErrors.ForbiddenCode, CollectionErrors.NotFoundCode, CollectionErrors.NameAlreadyExistsCode)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCollectionRequest request)
    {
        var command = new UpdateCollectionCommand(
            _currentUser.UserId.Value,
            id,
            request.Name,
            request.Description,
            request.IsPublic);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(List<CollectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCollections(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var query = new GetUserCollectionsQuery(userId, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    [ApiErrors(CollectionErrors.NotFoundCode, CollectionErrors.ForbiddenCode)]
    public async Task<IActionResult> GetItems(Guid id, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 20)
    {
        var query = new GetCollectionItemsQuery(id, _currentUser.UserId.Value, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/v2")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    [ApiErrors(CollectionErrors.NotFoundCode, CollectionErrors.ForbiddenCode)]
    public async Task<IActionResult> GetItemsV2(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var query = new GetCollectionItemsV2Query(id, _currentUser.UserId.Value, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/items/{pubId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(CollectionErrors.NotFoundCode, PublicationErrors.NotFoundCode, CollectionErrors.ForbiddenCode)]
    public async Task<IActionResult> AddItem(Guid id, Guid pubId)
    {
        var command = new AddToCollectionCommand(_currentUser.UserId.Value, id, pubId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}/items/{pubId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(CollectionErrors.NotFoundCode, PublicationErrors.NotFoundCode, CollectionErrors.ForbiddenCode)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid pubId)
    {
        var command = new RemoveFromCollectionCommand(_currentUser.UserId.Value, id, pubId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(CollectionErrors.ForbiddenCode, CollectionErrors.NotFoundCode)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCollectionCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
