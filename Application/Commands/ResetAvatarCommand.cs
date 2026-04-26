using ErrorOr;
using MediatR;

namespace Application.Commands;

public record ResetAvatarCommand(Guid UserId) : IRequest<ErrorOr<Updated>>;
