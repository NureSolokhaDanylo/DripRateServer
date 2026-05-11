using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Moderation;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Moderation;

public sealed class GetReportedEntitiesQueryHandler : IRequestHandler<GetReportedEntitiesQuery, ErrorOr<List<ReportedEntityDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetReportedEntitiesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ErrorOr<List<ReportedEntityDto>>> Handle(GetReportedEntitiesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.IsInRole("Moderator"))
        {
            return ReportErrors.Unauthorized;
        }

        var moderatorId = _currentUser.UserId!.Value;

        // 1. Check if moderator already has assigned reports in review
        var existingReports = await _context.Reports
            .Include(r => r.AssignedToUser)
            .Where(r => (r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview) && r.AssignedToUserId == moderatorId)
            .ToListAsync(cancellationToken);

        if (existingReports.Any())
        {
            // Group existing reports to find targets
            var targetsUnderReview = existingReports
                .GroupBy(r => new { r.TargetType, r.TargetId })
                .Select(g => new { g.Key.TargetType, g.Key.TargetId })
                .ToList();

            var targetTypes = targetsUnderReview.Select(t => t.TargetType).Distinct().ToList();
            var targetIds = targetsUnderReview.Select(t => t.TargetId).Distinct().ToList();

            // Find any NEW unassigned reports for these same targets and assign them to this moderator
            var newReportsForExistingTargets = await _context.Reports
                .Where(r => r.Status == ReportStatus.Pending 
                         && r.AssignedToUserId == null
                         && targetTypes.Contains(r.TargetType) 
                         && targetIds.Contains(r.TargetId))
                .ToListAsync(cancellationToken);

            var exactNewReports = newReportsForExistingTargets
                .Where(r => targetsUnderReview.Any(t => t.TargetType == r.TargetType && t.TargetId == r.TargetId))
                .ToList();

            if (exactNewReports.Any())
            {
                foreach (var report in exactNewReports)
                {
                    report.AssignTo(moderatorId);
                }
                await _context.SaveChangesAsync(cancellationToken);
                existingReports.AddRange(exactNewReports);
            }

            return GroupAndMapReports(existingReports);
        }

        // 2. If no existing assignments, find a new batch of pending reports
        // We pick entities where ALL pending/in-review reports are unassigned.
        var newBatchTargets = await _context.Reports
            .Where(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview)
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .Where(g => g.Count(r => r.AssignedToUserId != null) == 0)
            .OrderByDescending(g => g.Count())
            .Take(request.Take)
            .Select(g => new { g.Key.TargetType, g.Key.TargetId })
            .ToListAsync(cancellationToken);

        if (!newBatchTargets.Any())
        {
            return new List<ReportedEntityDto>();
        }

        // 3. Fetch all reports for these targets and assign to the moderator
        var targetTypesBatch = newBatchTargets.Select(t => t.TargetType).Distinct().ToList();
        var targetIdsBatch = newBatchTargets.Select(t => t.TargetId).Distinct().ToList();

        var reportsToAssign = await _context.Reports
            .Include(r => r.AssignedToUser)
            .Where(r => (r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview)
                     && targetTypesBatch.Contains(r.TargetType) 
                     && targetIdsBatch.Contains(r.TargetId))
            .ToListAsync(cancellationToken);
            
        var exactReportsToAssign = reportsToAssign
            .Where(r => newBatchTargets.Any(t => t.TargetType == r.TargetType && t.TargetId == r.TargetId))
            .ToList();

        foreach (var report in exactReportsToAssign)
        {
            report.AssignTo(moderatorId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        return GroupAndMapReports(exactReportsToAssign);
    }

    private List<ReportedEntityDto> GroupAndMapReports(IEnumerable<Report> reports)
    {
        return reports
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .Select(g =>
            {
                var mostCommonCategory = g.GroupBy(r => r.Category)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .FirstOrDefault();

                var assignedUser = g.FirstOrDefault(r => r.AssignedToUser != null)?.AssignedToUser;

                return new ReportedEntityDto(
                    g.Key.TargetType,
                    g.Key.TargetId,
                    g.Count(),
                    mostCommonCategory,
                    g.Min(r => r.CreatedAt),
                    g.Max(r => r.CreatedAt),
                    g.Max(r => r.AssignedToUserId),
                    assignedUser?.DisplayName
                );
            })
            .ToList();
    }
}
