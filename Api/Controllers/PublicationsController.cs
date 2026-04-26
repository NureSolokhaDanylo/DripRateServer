using Api.Attributes;
using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[AuthorizeWithError]
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
    [ApiErrors(ClothErrors.NotFoundCode, FileErrors.ProcessingFailedCode)]
    public async Task<IActionResult> Create([FromForm] CreatePublicationRequest request)
    {
        var images = new List<ImageUploadData>();
        
        if (request.Image != null)
        {
            images.Add(new ImageUploadData(request.Image.OpenReadStream(), request.Image.ContentType, request.Image.FileName));
        }

        if (request.Images != null)
        {
            foreach (var file in request.Images)
            {
                images.Add(new ImageUploadData(file.OpenReadStream(), file.ContentType, file.FileName));
            }
        }

        var command = new CreatePublicationCommand(
            _currentUser.UserId.Value,
            request.Description,
            images,
            request.TagIds,
            request.ClothIds);

        var result = await _mediator.Send(command);

        foreach (var img in images)
        {
            await img.Stream.DisposeAsync();
        }

        return result.Match(
            id => CreatedAtAction(nameof(Get), new { id }, id),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PublicationResponse), StatusCodes.Status200OK)]
    [ApiErrors(PublicationErrors.NotFoundCode)]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetPublicationQuery(id, _currentUser.UserId);
        var result = await _mediator.Send(query);
 
        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(PublicationErrors.ForbiddenCode, PublicationErrors.NotFoundCode)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeletePublicationCommand(id, _currentUser.UserId.Value);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ApiErrors(PublicationErrors.NotFoundCode, CollectionErrors.LikesNotInitializedCode)]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var command = new ToggleLikeCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            isLiked => Ok(isLiked),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/save")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ApiErrors(PublicationErrors.NotFoundCode, CollectionErrors.LikesNotInitializedCode)]
    public async Task<IActionResult> ToggleSave(Guid id)
    {
        var command = new ToggleSaveCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            isSaved => Ok(isSaved),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ApiErrors(PublicationErrors.NotFoundCode, CommentErrors.ParentNotFoundCode)]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentRequest request)
    {
        var command = new CreateCommentCommand(_currentUser.UserId.Value, id, request.Text, request.ParentCommentId);
        var result = await _mediator.Send(command);

        return result.Match(
            commentId => CreatedAtAction(nameof(GetComments), new { id }, commentId),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(
        Guid id, 
        [FromQuery] Guid? parentCommentId, 
        [FromQuery] DateTimeOffset? cursor, 
        [FromQuery] int take = 30)
    {
        var query = new GetCommentsQuery(id, _currentUser.UserId, parentCommentId, cursor, take);
        var result = await _mediator.Send(query);
 
        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}/comments/{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(CommentErrors.NotFoundCode, CommentErrors.ForbiddenCode)]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var command = new DeleteCommentCommand(_currentUser.UserId.Value, commentId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/comments/{commentId:guid}/like")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ApiErrors(CommentErrors.NotFoundCode)]
    public async Task<IActionResult> ToggleCommentLike(Guid id, Guid commentId)
    {
        var command = new ToggleCommentLikeCommand(_currentUser.UserId.Value, commentId);
        var result = await _mediator.Send(command);

        return result.Match(
            isLiked => Ok(isLiked),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/assessments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiErrors(PublicationErrors.NotFoundCode, AssessmentErrors.CannotRateOwnPublicationCode)]
    public async Task<IActionResult> CreateAssessment(Guid id, [FromBody] CreateAssessmentRequest request)
    {
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

    [HttpGet("{id:guid}/assessments/list")]
    [ProducesResponseType(typeof(List<IndividualAssessmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssessmentsList(Guid id, [FromQuery] DateTimeOffset? cursor, [FromQuery] int take = 30)
    {
        var query = new GetPublicationAssessmentsListQuery(id, _currentUser.UserId, cursor, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }
}
