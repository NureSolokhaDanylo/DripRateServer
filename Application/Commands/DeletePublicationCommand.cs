using MediatR;
using ErrorOr;

namespace Application.Commands;

public record DeletePublicationCommand(Guid PublicationId, Guid UserId) : IRequest<ErrorOr<Deleted>>;
