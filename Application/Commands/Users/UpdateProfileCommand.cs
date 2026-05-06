using ErrorOr;
using MediatR;

namespace Application.Commands.Users;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Bio) : IRequest<ErrorOr<Updated>>;
