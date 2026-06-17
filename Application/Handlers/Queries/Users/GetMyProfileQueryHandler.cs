using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Users;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Users;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<UserProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetMyProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserProfileResponse>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(UserProfileResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return UserErrors.NotFound;
        }

        return profile;
    }
}
