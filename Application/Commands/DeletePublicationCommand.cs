using MediatR;
using ErrorOr;

namespace Application.Commands;

public record DeletePublicationCommand(Guid PublicationId) : IRequest<ErrorOr<Deleted>>;
