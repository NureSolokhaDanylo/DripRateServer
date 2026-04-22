using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, ErrorOr<List<TagResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<TagResponse>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .Select(t => new TagResponse(t.Id, t.Name, t.Category))
            .ToListAsync(cancellationToken);

        return result;
    }
}
