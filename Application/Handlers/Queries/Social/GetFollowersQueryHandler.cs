using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Social;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Social;

public sealed class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, ErrorOr<List<UserProfileResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetFollowersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<UserProfileResponse>>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FolloweeId == request.UserId)
            .OrderBy(f => f.Follower.DisplayName)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(f => new UserProfileResponse(
                f.FollowerId,
                f.Follower.DisplayName,
                f.Follower.Bio,
                f.Follower.AvatarUrl,
                f.Follower.Followers.Count,
                f.Follower.Following.Count,
                f.Follower.Publications.Count,
                false
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
