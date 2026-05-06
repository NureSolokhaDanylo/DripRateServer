using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Assessments;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Assessments;

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
