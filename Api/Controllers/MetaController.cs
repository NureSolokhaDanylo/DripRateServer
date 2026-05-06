using Application.Dtos;
using Application.Queries;
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
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public sealed class MetaController : ApiController
{
    private readonly ISender _mediator;

    public MetaController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags()
    {
        var query = new GetTagsQuery();
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
