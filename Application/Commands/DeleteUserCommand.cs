using MediatR;
using ErrorOr;

namespace Application.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<ErrorOr<Deleted>>;
