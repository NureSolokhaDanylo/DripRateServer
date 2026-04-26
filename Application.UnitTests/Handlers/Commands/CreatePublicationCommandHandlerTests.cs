using Application.Commands;
using Application.Handlers.Commands;
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
