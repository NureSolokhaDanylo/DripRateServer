using Api.Attributes;
using Application.Commands.Advertisements;
using Application.Queries.Advertisements;
using Application.Dtos;
using Application.Interfaces;
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

    [HttpGet]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(typeof(List<AdvertisementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var query = new GetAdvertisementsQuery(isActive, search, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost]
    [AuthorizeWithError(Roles = "Moderator")]
    [ProducesResponseType(typeof(AdvertisementResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] CreateAdvertisementRequest request)
    {
        var command = new CreateAdvertisementCommand(
            request.Text,
            request.Url,
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
            request.Url,
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
    [ProducesResponseType(typeof(AdvertisementResponse), StatusCodes.Status200OK)]
    [ApiErrors(AdvertisementErrors.NotFoundCode, AdvertisementErrors.LimitReachedCode)]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] bool isActive)
    {
        var command = new ToggleAdvertisementActiveCommand(id, isActive);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
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
