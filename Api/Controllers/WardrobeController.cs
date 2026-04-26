using Api.Attributes;
using Application.Commands;
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

    [HttpGet]
    [ProducesResponseType(typeof(List<ClothResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWardrobe(
        [FromQuery] string? query, 
        [FromQuery] string? sortBy = "newest",
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 20)
    {
        var q = new GetWardrobeQuery(_currentUser.UserId.Value, query, sortBy, skip, take);
        var result = await _mediator.Send(q);

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
