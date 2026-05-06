using ErrorOr;
using MediatR;

namespace Application.Commands.Auth;

public record ChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword) : IRequest<ErrorOr<Success>>;
