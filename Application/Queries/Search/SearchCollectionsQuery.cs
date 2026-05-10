using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Search;

public record SearchCollectionsQuery(
    string? SearchQuery,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<CollectionResponse>>>;
