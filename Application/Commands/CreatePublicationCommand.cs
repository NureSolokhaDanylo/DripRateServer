using ErrorOr;
using MediatR;

namespace Application.Commands;

public record ImageUploadData(Stream Stream, string ContentType, string FileName);

public record CreatePublicationCommand(
    Guid UserId,
    string Description,
    List<ImageUploadData> Images,
    List<Guid>? TagIds,
    List<Guid>? ClothIds) : IRequest<ErrorOr<Guid>>;
