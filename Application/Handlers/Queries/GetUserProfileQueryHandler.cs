using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
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
        var result = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserName == request.Username)
            .Select(UserProfileResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        return result;
    }
}
