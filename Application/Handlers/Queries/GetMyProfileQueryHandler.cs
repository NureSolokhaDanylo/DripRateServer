using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<UserProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetMyProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserProfileResponse>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileResponse(
                u.Id,
                u.UserName ?? string.Empty,
                u.Bio,
                u.AvatarUrl,
                u.Followers.Count,
                u.Following.Count,
                u.Publications.Count
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return Error.NotFound(description: "Profile not found.");
        }

        return result;
    }
}
