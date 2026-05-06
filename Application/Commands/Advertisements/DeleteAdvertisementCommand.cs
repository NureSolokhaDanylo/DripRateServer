using ErrorOr;
using MediatR;

namespace Application.Commands.Advertisements;

public record DeleteAdvertisementCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
