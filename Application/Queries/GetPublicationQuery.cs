using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetPublicationQuery(Guid Id, Guid? UserId = null) : IRequest<ErrorOr<PublicationResponse>>;
