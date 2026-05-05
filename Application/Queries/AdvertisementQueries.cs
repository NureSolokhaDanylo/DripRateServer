using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetAdvertisementQuery(Guid Id) : IRequest<ErrorOr<AdvertisementResponse>>;

public record GetAdvertisementsQuery(int Skip = 0, int Take = 20) : IRequest<ErrorOr<List<AdvertisementResponse>>>;
