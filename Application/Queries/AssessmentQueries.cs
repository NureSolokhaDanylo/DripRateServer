using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetPublicationAssessmentsQuery(Guid PublicationId) : IRequest<ErrorOr<AssessmentResponse>>;
