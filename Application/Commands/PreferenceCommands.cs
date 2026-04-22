using ErrorOr;
using MediatR;

namespace Application.Commands;

public record SetPreferencesCommand(Guid UserId, List<Guid> TagIds) : IRequest<ErrorOr<Success>>;
