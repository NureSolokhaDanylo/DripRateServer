using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetMyPreferencesQueryHandler : IRequestHandler<GetMyPreferencesQuery, ErrorOr<List<TagResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetMyPreferencesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<TagResponse>>> Handle(GetMyPreferencesQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.PreferredTags)
            .Select(t => new TagResponse(t.Id, t.Name, t.Category))
            .ToListAsync(cancellationToken);

        return result;
    }
}
