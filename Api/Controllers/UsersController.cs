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
public sealed class UsersController : ApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUser _currentUser;

    public UsersController(ISender mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("@me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var query = new GetMyProfileQuery(_currentUser.UserId.Value);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPatch("@me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new UpdateProfileCommand(
            _currentUser.UserId.Value,
            request.Bio);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpPatch("@me/avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        using var stream = file.OpenReadStream();
        var command = new UploadAvatarCommand(
            _currentUser.UserId.Value,
            stream,
            file.ContentType,
            file.FileName);

        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int take = 20)
    {
        var q = new SearchUsersQuery(query, take);
        var result = await _mediator.Send(q);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var query = new GetUserProfileQuery(username);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Follow(Guid id)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new FollowUserCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unfollow(Guid id)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new UnfollowUserCommand(_currentUser.UserId.Value, id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/followers")]
    [ProducesResponseType(typeof(List<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowers(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var query = new GetFollowersQuery(id, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}/following")]
    [ProducesResponseType(typeof(List<UserProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowing(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var query = new GetFollowingQuery(id, skip, take);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpGet("@me/preferences")]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPreferences()
    {
        if (_currentUser.UserId == null) return Unauthorized();

        // Let's create GetMyPreferencesQuery for cleanliness.
        var query = new GetMyPreferencesQuery(_currentUser.UserId.Value);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPut("@me/preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetMyPreferences([FromBody] List<Guid> tagIds)
    {
        if (_currentUser.UserId == null) return Unauthorized();

        var command = new SetPreferencesCommand(_currentUser.UserId.Value, tagIds);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
}
