using ErrorOr;
using MediatR;

namespace Application.Commands.Advertisements;

public record ToggleAdvertisementActiveCommand(Guid Id, bool IsActive) : IRequest<ErrorOr<Success>>;
