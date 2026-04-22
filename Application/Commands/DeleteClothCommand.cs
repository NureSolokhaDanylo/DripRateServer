using ErrorOr;
using MediatR;

namespace Application.Commands;

public record DeleteClothCommand(Guid UserId, Guid ClothId) : IRequest<ErrorOr<Deleted>>;
