using ErrorOr;
using MediatR;

namespace Application.Commands.Users;

public record ResetAvatarCommand(Guid UserId) : IRequest<ErrorOr<Updated>>;
