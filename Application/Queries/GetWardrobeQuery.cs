using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetWardrobeQuery(
    Guid UserId,
    string? SearchQuery,
    string? SortBy = "newest",
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<ClothResponseDto>>>;
