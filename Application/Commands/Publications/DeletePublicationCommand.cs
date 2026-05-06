using MediatR;
using ErrorOr;

namespace Application.Commands.Publications;

public record DeletePublicationCommand(Guid PublicationId, Guid UserId) : IRequest<ErrorOr<Deleted>>;
