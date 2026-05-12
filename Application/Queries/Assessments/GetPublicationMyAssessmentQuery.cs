using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Assessments;

public record GetPublicationMyAssessmentQuery(Guid PublicationId, Guid UserId) : IRequest<ErrorOr<IndividualAssessmentResponse>>;
