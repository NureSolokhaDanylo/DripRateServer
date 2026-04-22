using ErrorOr;
using MediatR;

namespace Application.Commands;

public record CreatePublicationCommand(
    Guid UserId,
    string Description,
    Stream ImageStream,
    string ContentType,
    string FileName,
    List<Guid>? TagIds,
    List<Guid>? ClothIds) : IRequest<ErrorOr<Guid>>;
