using Application.Commands;
using Application.Commands.Advertisements;
using Application.Commands.Auth;
using Application.Commands.Collections;
using Application.Commands.Comments;
using Application.Commands.Games;
using Application.Commands.Publications;
using Application.Commands.Reports;
using Application.Commands.Social;
using Application.Commands.Users;
using Application.Commands.Wardrobe;
using Application.Handlers.Commands;
using Application.Handlers.Commands.Advertisements;
using Application.Handlers.Commands.Auth;
using Application.Handlers.Commands.Collections;
using Application.Handlers.Commands.Comments;
using Application.Handlers.Commands.Games;
using Application.Handlers.Commands.Publications;
using Application.Handlers.Commands.Reports;
using Application.Handlers.Commands.Social;
using Application.Handlers.Commands.Users;
using Application.Handlers.Commands.Wardrobe;
using Application.Interfaces;
using Application.Interfaces.Internal;
using ErrorOr;
using Moq;
using Xunit;

namespace Application.UnitTests.Handlers.Commands;

public sealed class CreatePublicationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSecondUploadFails_DeletesPreviouslyUploadedImages()
    {
        var dbContextMock = new Mock<IApplicationDbContext>(MockBehavior.Strict);
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        var fileStorageServiceMock = new Mock<IFileStorageService>(MockBehavior.Strict);
        var ct = CancellationToken.None;

        var firstImageUrl = "https://cdn.example.com/publications/first.jpg";
        var uploadCallIndex = 0;

        fileServiceMock
            .Setup(s => s.UploadPublicationImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                uploadCallIndex++;
                if (uploadCallIndex == 1)
                {
                    return (ErrorOr<string>)firstImageUrl;
                }

                return (ErrorOr<string>)Error.Unexpected("Upload.Failed", "upload failed");
            });

        fileStorageServiceMock
            .Setup(s => s.DeleteFileAsync(firstImageUrl, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePublicationCommandHandler(
            dbContextMock.Object,
            fileServiceMock.Object,
            fileStorageServiceMock.Object);

        var command = new CreatePublicationCommand(
            Guid.NewGuid(),
            "test publication",
            [
                new ImageUploadData(new MemoryStream([1, 2, 3]), "image/jpeg", "first.jpg"),
                new ImageUploadData(new MemoryStream([4, 5, 6]), "image/jpeg", "second.jpg")
            ],
            null,
            null);

        var result = await handler.Handle(command, ct);

        Assert.True(result.IsError);
        fileStorageServiceMock.Verify(s => s.DeleteFileAsync(firstImageUrl, It.IsAny<CancellationToken>()), Times.Once);
        fileStorageServiceMock.VerifyNoOtherCalls();
        dbContextMock.VerifyNoOtherCalls();
    }
}
