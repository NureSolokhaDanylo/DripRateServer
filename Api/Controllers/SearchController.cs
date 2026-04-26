using Api.Attributes;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError]
[Route("api/[controller]")]
public sealed class SearchController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public SearchController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("publications")]
    [ProducesResponseType(typeof(List<PublicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPublications(
        [FromQuery] string? query, 
        [FromQuery] List<Guid>? tags, 
        [FromQuery] DateTimeOffset? cursor, 
        [FromQuery] int take = 20)
    {
        var q = new SearchPublicationsQuery(query, tags, _currentUser.UserId, cursor, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var q = new SearchUsersQuery(query, skip, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("collections")]
    [ProducesResponseType(typeof(List<CollectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCollections([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var q = new SearchCollectionsQuery(query, skip, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
