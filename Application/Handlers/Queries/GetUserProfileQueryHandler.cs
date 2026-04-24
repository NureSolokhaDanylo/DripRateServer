using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ErrorOr<UserProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Publications)
            .FirstOrDefaultAsync(u => u.DisplayName == request.Username, cancellationToken);

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        var isFollowing = request.CurrentUserId.HasValue && 
                          user.Followers.Any(f => f.FollowerId == request.CurrentUserId.Value);

        return new UserProfileResponse(
            user.Id,
            user.DisplayName,
            user.Bio,
            user.AvatarUrl,
            user.Followers.Count,
            user.Following.Count,
            user.Publications.Count,
            isFollowing);
    }
}
