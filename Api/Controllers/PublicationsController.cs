using Api.Attributes;
using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class PublicationsController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public PublicationsController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreatePublicationRequest request)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        using var stream = request.Image.OpenReadStream();
        var command = new CreatePublicationCommand(
            _currentUser.UserId.Value,
            request.Description,
            stream,
            request.Image.ContentType,
            request.Image.FileName,
            request.TagIds,
            request.ClothIds);

        var result = await _mediator.Send(command);

        return result.Match(
            id => CreatedAtAction(nameof(Get), new { id }, id),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PublicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiErrors(StatusCodes.Status404NotFound, "Publication.NotFound")]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetPublicationQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiErrors(StatusCodes.Status403Forbidden, "Publication.Forbidden")]
    [ApiErrors(StatusCodes.Status404NotFound, "Publication.NotFound")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new DeletePublicationCommand(id, _currentUser.UserId.Value);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new ToggleLikeCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            isLiked => Ok(isLiked),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentRequest request)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new CreateCommentCommand(_currentUser.UserId.Value, id, request.Text, request.ParentCommentId);
        var result = await _mediator.Send(command);

        return result.Match(
            commentId => CreatedAtAction(nameof(GetComments), new { id }, commentId),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 30)
    {
        var query = new GetCommentsQuery(id, _currentUser.UserId, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/comments/{commentId:guid}/like")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleCommentLike(Guid id, Guid commentId)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new ToggleCommentLikeCommand(_currentUser.UserId.Value, commentId);
        var result = await _mediator.Send(command);

        return result.Match(
            isLiked => Ok(isLiked),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/assessments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssessment(Guid id, [FromBody] CreateAssessmentRequest request)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new CreateAssessmentCommand(
            _currentUser.UserId.Value,
            id,
            request.ColorCoordination,
            request.FitAndProportions,
            request.Originality,
            request.OverallStyle);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/assessments")]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssessments(Guid id)
    {
        var query = new GetPublicationAssessmentsQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
