using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Collections;

public record GetCollectionItemsV2Query(Guid CollectionId, Guid UserId, int Skip = 0, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
