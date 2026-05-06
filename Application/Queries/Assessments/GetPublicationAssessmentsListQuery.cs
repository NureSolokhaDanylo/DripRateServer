using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Assessments;

public record GetPublicationAssessmentsListQuery(
    Guid PublicationId,
    Guid? UserId = null,
    DateTimeOffset? Cursor = null,
    int Take = 30) : IRequest<ErrorOr<List<IndividualAssessmentResponse>>>;
