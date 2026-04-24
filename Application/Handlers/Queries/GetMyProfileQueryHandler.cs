using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
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
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.Publications)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        return new UserProfileResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Bio,
            user.AvatarUrl,
            user.Followers.Count,
            user.Following.Count,
            user.Publications.Count,
            false);
    }
}
