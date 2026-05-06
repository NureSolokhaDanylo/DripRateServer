using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Advertisements;

public record GetAdvertisementQuery(Guid Id) : IRequest<ErrorOr<AdvertisementResponse>>;
