using Api.Attributes;
using Application.Commands;
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
