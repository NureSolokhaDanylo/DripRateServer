using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Wardrobe;

public record GetClothByIdQuery(Guid Id) : IRequest<ErrorOr<ClothResponseDto>>;
