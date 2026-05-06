using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Social;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Social;

public sealed class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, ErrorOr<List<UserProfileResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetFollowingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<UserProfileResponse>>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == request.UserId)
            .OrderBy(f => f.Followee.DisplayName)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(f => new UserProfileResponse(
                f.FolloweeId,
                f.Followee.DisplayName,
                f.Followee.Bio,
                f.Followee.AvatarUrl,
                f.Followee.Followers.Count,
                f.Followee.Following.Count,
                f.Followee.Publications.Count,
                false
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
