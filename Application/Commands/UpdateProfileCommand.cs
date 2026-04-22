using ErrorOr;
using MediatR;

namespace Application.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string? Bio) : IRequest<ErrorOr<Updated>>;
