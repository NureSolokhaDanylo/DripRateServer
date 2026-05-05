using Api.Attributes;
using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError(Roles = "Moderator")]
[Route("api/[controller]")]
public sealed class ModerationController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public ModerationController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("reports")]
    [ProducesResponseType(typeof(List<ReportedEntityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportedEntities([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        var query = new GetReportedEntitiesQuery(skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("reports/{targetType}/{targetId:guid}")]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityReports(ReportTargetType targetType, Guid targetId)
    {
        var query = new GetEntityReportsQuery(targetType, targetId);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("reports/assign/{targetType}/{targetId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignReports(ReportTargetType targetType, Guid targetId)
    {
        var command = new AssignReportedEntityCommand(_currentUser.UserId!.Value, targetType, targetId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPost("reports/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResolveReports([FromBody] ResolveReportedEntityRequest request)
    {
        var command = new ResolveReportedEntityCommand(
            _currentUser.UserId!.Value,
            request.TargetType,
            request.TargetId,
            request.Action);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
