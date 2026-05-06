using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Moderation;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Moderation;

public sealed class GetEntityReportsQueryHandler : IRequestHandler<GetEntityReportsQuery, ErrorOr<List<ReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEntityReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<ReportDto>>> Handle(GetEntityReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .IgnoreQueryFilters()
            .Where(r => r.TargetType == request.TargetType && r.TargetId == request.TargetId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportDto(
                r.Id,
                r.Category,
                r.Text,
                r.AuthorId,
                r.Author.DisplayName,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return reports;
    }
}
