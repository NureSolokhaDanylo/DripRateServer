using ErrorOr;
using MediatR;

namespace Application.Commands;

public record ChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword) : IRequest<ErrorOr<Success>>;
