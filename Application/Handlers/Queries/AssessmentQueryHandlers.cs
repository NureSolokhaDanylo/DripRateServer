using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetPublicationAssessmentsQueryHandler : IRequestHandler<GetPublicationAssessmentsQuery, ErrorOr<AssessmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationAssessmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AssessmentResponse>> Handle(GetPublicationAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var assessments = _context.Assessments
            .AsNoTracking()
            .Where(a => a.PublicationId == request.PublicationId);

        var count = await assessments.CountAsync(cancellationToken);
        if (count == 0)
        {
            return new AssessmentResponse(0, 0, 0, 0, 0, 0);
        }

        var stats = await assessments
            .GroupBy(a => a.PublicationId)
            .Select(g => new
            {
                AvgColor = g.Average(a => (double)a.ColorCoordination),
                AvgFit = g.Average(a => (double)a.FitAndProportions),
                AvgOrig = g.Average(a => (double)a.Originality),
                AvgStyle = g.Average(a => (double)a.OverallStyle)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats == null) return new AssessmentResponse(0, 0, 0, 0, 0, 0);

        var totalAvg = (stats.AvgColor + stats.AvgFit + stats.AvgOrig + stats.AvgStyle) / 4.0;

        return new AssessmentResponse(
            stats.AvgColor,
            stats.AvgFit,
            stats.AvgOrig,
            stats.AvgStyle,
            totalAvg,
            count);
    }
}

public sealed class GetPublicationAssessmentsListQueryHandler : IRequestHandler<GetPublicationAssessmentsListQuery, ErrorOr<List<IndividualAssessmentResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationAssessmentsListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<IndividualAssessmentResponse>>> Handle(GetPublicationAssessmentsListQuery request, CancellationToken cancellationToken)
    {
        var followingIds = request.UserId.HasValue 
            ? await _context.Follows
                .Where(f => f.FollowerId == request.UserId.Value)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        var query = _context.Assessments
            .AsNoTracking()
            .Where(a => a.PublicationId == request.PublicationId);

        if (request.Cursor.HasValue)
        {
            query = query.Where(a => a.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(a => followingIds.Contains(a.UserId))
            .ThenByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Take(request.Take)
            .Select(a => new IndividualAssessmentResponse(
                a.UserId,
                a.User.DisplayName,
                a.User.AvatarUrl,
                a.ColorCoordination,
                a.FitAndProportions,
                a.Originality,
                a.OverallStyle,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return result;
    }
}
