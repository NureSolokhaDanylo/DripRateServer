using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, ErrorOr<List<UserProfileResponse>>>
{
    private readonly IApplicationDbContext _context;

    public SearchUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<UserProfileResponse>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchQuery.ToLower();

        var result = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserName != null && u.UserName.ToLower().Contains(search))
            .Take(request.Take)
            .Select(UserProfileResponse.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }
}
