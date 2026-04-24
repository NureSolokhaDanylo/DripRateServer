using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Domain;
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
        var publicationResult = await _context.Publications.GetByIdOrErrorAsync(request.PublicationId, cancellationToken);
        if (publicationResult.IsError) return publicationResult.Errors;

        var publication = publicationResult.Value;

        if (publication.UserId == request.UserId)
        {
            return Error.Validation(description: "You cannot rate your own publication.");
        }
...
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.PublicationId == request.PublicationId, cancellationToken);

        if (assessment != null)
        {
            assessment.UpdateRatings(
                request.ColorCoordination,
                request.FitAndProportions,
                request.Originality,
                request.OverallStyle);
        }
        else
        {
            assessment = new Assessment(
                request.UserId,
                request.PublicationId,
                request.ColorCoordination,
                request.FitAndProportions,
                request.Originality,
                request.OverallStyle);
            
            _context.Assessments.Add(assessment);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
