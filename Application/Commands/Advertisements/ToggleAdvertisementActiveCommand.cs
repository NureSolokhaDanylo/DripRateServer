using ErrorOr;
using MediatR;
using Application.Dtos;

namespace Application.Commands.Advertisements;

public record ToggleAdvertisementActiveCommand(Guid Id, bool IsActive) : IRequest<ErrorOr<AdvertisementResponse>>;
