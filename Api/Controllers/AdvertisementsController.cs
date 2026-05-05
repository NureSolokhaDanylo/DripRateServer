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
public sealed class AdvertisementsController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public AdvertisementsController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(typeof(AdvertisementResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] CreateAdvertisementRequest request)
    {
        var command = new CreateAdvertisementCommand(
            request.Text,
            request.MaxImpressions,
            request.Images,
            request.TagIds);

        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(Get), new { id = response.Id }, response),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdvertisementResponse), StatusCodes.Status200OK)]
    [ApiErrors(AdvertisementErrors.NotFoundCode)]
    public async Task<IActionResult> Get(Guid id)
    {
        // For simplicity, we just return the ad. Usually you'd have a GetAdvertisementQuery.
        // But here I'll just use a placeholder or implement the query.
        // Let's implement GetAdvertisementQuery for completeness.
        var query = new GetAdvertisementQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPut("{id:guid}")]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(typeof(AdvertisementResponse), StatusCodes.Status200OK)]
    [ApiErrors(AdvertisementErrors.NotFoundCode)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateAdvertisementRequest request)
    {
        var command = new UpdateAdvertisementCommand(
            id,
            request.Text,
            request.MaxImpressions,
            request.ExistingImages,
            request.NewImages,
            request.TagIds,
            request.IsActive);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPatch("{id:guid}/active")]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(AdvertisementErrors.NotFoundCode, AdvertisementErrors.LimitReachedCode)]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] bool isActive)
    {
        var command = new ToggleAdvertisementActiveCommand(id, isActive);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(AdvertisementErrors.NotFoundCode)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteAdvertisementCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/view")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(AdvertisementErrors.NotFoundCode)]
    public async Task<IActionResult> RegisterView(Guid id)
    {
        var command = new ViewAdvertisementCommand(id, _currentUser.UserId.Value);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
