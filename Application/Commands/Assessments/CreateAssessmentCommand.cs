using ErrorOr;
using MediatR;

namespace Application.Commands.Assessments;

public record CreateAssessmentCommand(
    Guid UserId,
    Guid PublicationId,
    int ColorCoordination,
    int FitAndProportions,
    int Originality,
    int OverallStyle) : IRequest<ErrorOr<Success>>;
