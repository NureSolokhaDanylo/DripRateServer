using ErrorOr;
using MediatR;

namespace Application.Commands.Advertisements;

public record ViewAdvertisementCommand(Guid AdId, Guid UserId) : IRequest<ErrorOr<Success>>;
