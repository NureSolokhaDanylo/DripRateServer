using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class SearchController : ApiController
{
    private readonly ISender _mediator;

    public SearchController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("publications")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPublications(
        [FromQuery] string? query, 
        [FromQuery] List<Guid>? tags, 
        [FromQuery] DateTimeOffset? cursor, 
        [FromQuery] int take = 20)
    {
        var q = new SearchPublicationsQuery(query, tags, cursor, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int take = 20)
    {
        var q = new SearchUsersQuery(query, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("collections")]
    [ProducesResponseType(typeof(List<CollectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCollections([FromQuery] string query, [FromQuery] int take = 20)
    {
        var q = new SearchCollectionsQuery(query, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
