using ErrorOr;
using MediatR;

namespace Application.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Bio) : IRequest<ErrorOr<Updated>>;
