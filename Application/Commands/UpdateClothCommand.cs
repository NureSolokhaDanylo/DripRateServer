using ErrorOr;
using MediatR;

namespace Application.Commands;

public record UpdateClothCommand(
    Guid UserId,
    Guid ClothId,
    string Name,
    string? Brand,
    string? StoreLink,
    decimal? EstimatedPrice,
    Stream? PhotoStream,
    string? PhotoContentType,
    string? PhotoFileName) : IRequest<ErrorOr<Success>>;
