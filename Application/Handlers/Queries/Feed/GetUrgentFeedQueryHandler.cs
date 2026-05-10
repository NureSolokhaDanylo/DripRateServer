using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Feed;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Feed;

public sealed class GetUrgentFeedQueryHandler : IRequestHandler<GetUrgentFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUrgentFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetUrgentFeedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications
            .AsNoTracking()
            .Where(p => p.IsUrgentRatingRequested);

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
