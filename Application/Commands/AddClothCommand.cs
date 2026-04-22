using ErrorOr;
using MediatR;

namespace Application.Commands;

public record AddClothCommand(
    Guid UserId,
    string Name,
    string? Brand,
    string? StoreLink,
    decimal? EstimatedPrice,
    Stream? PhotoStream,
    string? PhotoContentType,
    string? PhotoFileName) : IRequest<ErrorOr<Guid>>;
