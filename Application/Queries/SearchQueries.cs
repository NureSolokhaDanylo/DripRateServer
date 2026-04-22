using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record SearchPublicationsQuery(
    string? SearchQuery,
    List<Guid>? TagIds,
    DateTimeOffset? Cursor,
    int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public record SearchCollectionsQuery(
    string SearchQuery,
    int Take = 20) : IRequest<ErrorOr<List<CollectionResponse>>>;
