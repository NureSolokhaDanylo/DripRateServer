using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Users;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Users;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ErrorOr<UserProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileResponse(
                u.Id,
                u.DisplayName,
                u.Bio,
                u.AvatarUrl,
                u.Followers.Count,
                u.Following.Count,
                u.Publications.Count,
                request.CurrentUserId.HasValue && u.Followers.Any(f => f.FollowerId == request.CurrentUserId.Value)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return UserErrors.NotFound;
        }

        return profile;
    }
}
