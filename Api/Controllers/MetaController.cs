using Application.Dtos;
using Application.Queries;
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
