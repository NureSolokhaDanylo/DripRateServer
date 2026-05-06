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
using Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError]
[Route("api/[controller]")]
public sealed class ReportsController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public ReportsController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(ReportErrors.InvalidTargetCode, ReportErrors.SelfReportCode, ReportErrors.DuplicateReportCode)]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request)
    {
        var command = new CreateReportCommand(
            _currentUser.UserId.Value,
            request.TargetType,
            request.TargetId,
            request.Category,
            request.Text);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
