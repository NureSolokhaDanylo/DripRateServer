using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Feed;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Queries.Feed;

public sealed class GetUserFeedQueryHandler : IRequestHandler<GetUserFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId);

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.CurrentUserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
