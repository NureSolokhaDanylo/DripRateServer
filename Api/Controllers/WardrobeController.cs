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
public sealed class WardrobeController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public WardrobeController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ApiErrors(FileErrors.ProcessingFailedCode)]
    public async Task<IActionResult> Add([FromForm] AddClothRequest request)
    {
        Stream? photoStream = null;
        if (request.Photo != null)
        {
            photoStream = request.Photo.OpenReadStream();
        }

        var command = new AddClothCommand(
            _currentUser.UserId.Value,
            request.Name,
            request.Brand,
            request.StoreLink,
            request.EstimatedPrice,
            photoStream,
            request.Photo?.ContentType,
            request.Photo?.FileName);

        var result = await _mediator.Send(command);

        if (photoStream != null) await photoStream.DisposeAsync();

        return result.Match(
            id => CreatedAtAction(nameof(Add), new { id }, id),
            errors => Problem(errors));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(ClothErrors.NotFoundCode, ClothErrors.ForbiddenCode, FileErrors.ProcessingFailedCode)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateClothRequest request)
    {
        Stream? photoStream = null;
        if (request.Photo != null)
        {
            photoStream = request.Photo.OpenReadStream();
        }

        var command = new UpdateClothCommand(
            _currentUser.UserId.Value,
            id,
            request.Name,
            request.Brand,
            request.StoreLink,
            request.EstimatedPrice,
            photoStream,
            request.Photo?.ContentType,
            request.Photo?.FileName);

        var result = await _mediator.Send(command);

        if (photoStream != null) await photoStream.DisposeAsync();

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ClothResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWardrobe(
        [FromQuery] Guid? userId,
        [FromQuery] string? query, 
        [FromQuery] string? sortBy = "newest",
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 20)
    {
        var targetUserId = userId ?? _currentUser.UserId!.Value;
        var q = new GetWardrobeQuery(targetUserId, query, sortBy, skip, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClothResponseDto), StatusCodes.Status200OK)]
    [ApiErrors(ClothErrors.NotFoundCode)]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetClothByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(ClothErrors.NotFoundCode, ClothErrors.ForbiddenCode)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteClothCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
