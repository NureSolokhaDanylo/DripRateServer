using ErrorOr;
using MediatR;

namespace Application.Commands;

public record UploadAvatarCommand(
    Guid UserId,
    Stream ImageStream,
    string ContentType,
    string FileName) : IRequest<ErrorOr<Updated>>;
