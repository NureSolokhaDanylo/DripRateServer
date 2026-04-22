using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetPublicationQuery(Guid Id) : IRequest<ErrorOr<PublicationResponse>>;
