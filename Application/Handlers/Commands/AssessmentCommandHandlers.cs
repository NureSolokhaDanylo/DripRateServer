using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class CreateAssessmentCommandHandler : IRequestHandler<CreateAssessmentCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public CreateAssessmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var publicationResult = await _context.Publications.GetByIdOrErrorAsync(
            request.PublicationId, 
            PublicationErrors.NotFound, 
            cancellationToken);
            
        if (publicationResult.IsError) return publicationResult.Errors;

        var publication = publicationResult.Value;

        if (publication.UserId == request.UserId)
        {
            return AssessmentErrors.CannotRateOwnPublication;
        }

        var existingAssessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.PublicationId == request.PublicationId && a.UserId == request.UserId, cancellationToken);

        if (existingAssessment != null)
        {
            var oldAssessmentClone = new Assessment(
                existingAssessment.UserId,
                existingAssessment.PublicationId,
                existingAssessment.ColorCoordination,
                existingAssessment.FitAndProportions,
                existingAssessment.Originality,
                existingAssessment.OverallStyle);

            existingAssessment.UpdateRatings(
                request.ColorCoordination,
                request.FitAndProportions,
                request.Originality,
                request.OverallStyle);

            publication.ApplyAssessment(oldAssessmentClone, existingAssessment);
        }
        else
        {
            var assessment = new Assessment(
                request.UserId,
                request.PublicationId,
                request.ColorCoordination,
                request.FitAndProportions,
                request.Originality,
                request.OverallStyle);

            _context.Assessments.Add(assessment);
            publication.ApplyAssessment(null, assessment);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
