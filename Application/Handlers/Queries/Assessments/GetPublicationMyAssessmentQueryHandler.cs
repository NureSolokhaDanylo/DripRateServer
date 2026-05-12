using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Assessments;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Assessments;

public sealed class GetPublicationMyAssessmentQueryHandler : IRequestHandler<GetPublicationMyAssessmentQuery, ErrorOr<IndividualAssessmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationMyAssessmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IndividualAssessmentResponse>> Handle(GetPublicationMyAssessmentQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _context.Assessments
            .AsNoTracking()
            .Where(a => a.PublicationId == request.PublicationId && a.UserId == request.UserId)
            .Select(a => new IndividualAssessmentResponse(
                a.UserId,
                a.User.DisplayName,
                a.User.AvatarUrl,
                a.ColorCoordination,
                a.FitAndProportions,
                a.Originality,
                a.OverallStyle,
                a.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (assessment is null)
        {
            return AssessmentErrors.NotFound;
        }

        return assessment;
    }
}
