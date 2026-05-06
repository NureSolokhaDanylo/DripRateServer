using ErrorOr;
using MediatR;

namespace Application.Commands.Wardrobe;

public record DeleteClothCommand(Guid UserId, Guid ClothId) : IRequest<ErrorOr<Deleted>>;
