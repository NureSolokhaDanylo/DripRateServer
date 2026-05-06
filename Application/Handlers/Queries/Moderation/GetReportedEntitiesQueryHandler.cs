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
        var existingAssignments = await GetGroupedReportsQuery()
            .Where(x => x.AssignedToUserId == moderatorId)
            .ToListAsync(cancellationToken);

        if (existingAssignments.Any())
        {
            return existingAssignments.Select(MapToDto).ToList();
        }

        // 2. If no existing assignments, find a new batch of pending reports
        var newBatchTargets = await _context.Reports
            .Where(r => r.Status == ReportStatus.Pending && r.AssignedToUserId == null)
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .OrderByDescending(g => g.Count())
            .Take(request.Take)
            .Select(g => new { g.Key.TargetType, g.Key.TargetId })
            .ToListAsync(cancellationToken);

        if (!newBatchTargets.Any())
        {
            return new List<ReportedEntityDto>();
        }

        // 3. Assign new batch to the moderator
        foreach (var target in newBatchTargets)
        {
            var reportsToAssign = await _context.Reports
                .Where(r => r.TargetType == target.TargetType && r.TargetId == target.TargetId && r.Status == ReportStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var report in reportsToAssign)
            {
                report.AssignTo(moderatorId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 4. Return the newly assigned batch
        var newlyAssigned = await GetGroupedReportsQuery()
            .Where(x => x.AssignedToUserId == moderatorId)
            .ToListAsync(cancellationToken);

        return newlyAssigned.Select(MapToDto).ToList();
    }

    private IQueryable<ReportGroupInfo> GetGroupedReportsQuery()
    {
        return _context.Reports
            .Where(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview)
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .Select(g => new ReportGroupInfo
            {
                TargetType = g.Key.TargetType,
                TargetId = g.Key.TargetId,
                PendingReportsCount = g.Count(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview),
                FirstReportedAt = g.Min(r => r.CreatedAt),
                LastReportedAt = g.Max(r => r.CreatedAt),
                MostCommonCategory = g.GroupBy(r => r.Category)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .FirstOrDefault(),
                AssignedToUserId = g.Max(r => r.AssignedToUserId),
                AssignedToUserName = g.Where(r => r.AssignedToUser != null).Select(r => r.AssignedToUser!.DisplayName).FirstOrDefault()
            });
    }

    private ReportedEntityDto MapToDto(ReportGroupInfo x) => new(
        x.TargetType,
        x.TargetId,
        x.PendingReportsCount,
        x.MostCommonCategory,
        x.FirstReportedAt,
        x.LastReportedAt,
        x.AssignedToUserId,
        x.AssignedToUserName);

    private sealed class ReportGroupInfo
    {
        public ReportTargetType TargetType { get; init; }
        public Guid TargetId { get; init; }
        public int PendingReportsCount { get; init; }
        public DateTimeOffset FirstReportedAt { get; init; }
        public DateTimeOffset LastReportedAt { get; init; }
        public ReportCategory MostCommonCategory { get; init; }
        public Guid? AssignedToUserId { get; init; }
        public string? AssignedToUserName { get; init; }
    }
}
