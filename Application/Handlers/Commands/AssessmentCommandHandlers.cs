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
        // 1. Retrieve and validate publication existence
        var publicationResult = await _context.Publications.GetByIdOrErrorAsync(
            request.PublicationId, 
            PublicationErrors.NotFound, 
            cancellationToken);
            
        if (publicationResult.IsError) return publicationResult.Errors;

        var publication = publicationResult.Value;

        // 2. Prevent self-assessment
        if (publication.UserId == request.UserId)
        {
            return AssessmentErrors.CannotRateOwnPublication;
        }

        // 3. Find existing assessment or prepare new one
        var existingAssessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.PublicationId == request.PublicationId && a.UserId == request.UserId, cancellationToken);

        if (existingAssessment != null)
        {
            // 4a. Update existing assessment
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
            // 4b. Create new assessment
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

        // 5. Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
