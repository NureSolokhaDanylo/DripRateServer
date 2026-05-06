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
using Domain;
using Domain.Errors;
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
    public async Task<IActionResult> GetReportedEntities([FromQuery] int take = 10)
    {
        var query = new GetReportedEntitiesQuery(take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("reports/{targetType}/{targetId:guid}")]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    [ApiErrors(ReportErrors.NotFoundCode)]
    public async Task<IActionResult> GetEntityReports(ReportTargetType targetType, Guid targetId)
    {
        var query = new GetEntityReportsQuery(targetType, targetId);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("reports/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(ReportErrors.NotFoundCode, ReportErrors.UnauthorizedCode, UserErrors.CannotBanModeratorCode)]
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
