using ErrorOr;
using MediatR;

namespace Application.Commands.Users;

public record SetPreferencesCommand(Guid UserId, List<Guid> TagIds) : IRequest<ErrorOr<Success>>;
