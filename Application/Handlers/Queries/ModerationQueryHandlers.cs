using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetReportedEntitiesQueryHandler : IRequestHandler<GetReportedEntitiesQuery, ErrorOr<List<ReportedEntityDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetReportedEntitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<ReportedEntityDto>>> Handle(GetReportedEntitiesQuery request, CancellationToken cancellationToken)
    {
        var groupedReports = await _context.Reports
            .Where(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview)
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .Select(g => new
            {
                g.Key.TargetType,
                g.Key.TargetId,
                PendingReportsCount = g.Count(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview),
                FirstReportedAt = g.Min(r => r.CreatedAt),
                LastReportedAt = g.Max(r => r.CreatedAt),
                // Find most common category
                MostCommonCategory = g.GroupBy(r => r.Category)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .FirstOrDefault(),
                // Get assignment info (if any report in group is assigned, they all should be or will be)
                AssignedToUserId = g.Max(r => r.AssignedToUserId),
                AssignedToUserName = g.Where(r => r.AssignedToUser != null).Select(r => r.AssignedToUser!.DisplayName).FirstOrDefault()
            })
            .OrderByDescending(x => x.PendingReportsCount)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var result = groupedReports.Select(x => new ReportedEntityDto(
            x.TargetType,
            x.TargetId,
            x.PendingReportsCount,
            x.MostCommonCategory,
            x.FirstReportedAt,
            x.LastReportedAt,
            x.AssignedToUserId,
            x.AssignedToUserName
        )).ToList();

        return result;
    }
}

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
