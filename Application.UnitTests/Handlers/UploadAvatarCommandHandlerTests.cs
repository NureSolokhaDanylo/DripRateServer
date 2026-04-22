using Application.Commands;
using Application.Handlers.Commands;
using Application.Interfaces;
using Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Handlers;

public sealed class UploadAvatarCommandHandlerTests
{
    private readonly DbContextOptions<MyDbContext> _options;

    public UploadAvatarCommandHandlerTests()
    {
        _options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Handle_ShouldUploadFileAndSetAvatarUrl_WhenUserExists()
    {
        // Arrange
        await using var context = new MyDbContext(_options);
        var user = new User("test@test.com", "testuser");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockStorage = new Mock<IFileStorageService>();
        var fakeUrl = "https://dripstore.blob.core.windows.net/avatars/test.jpg";
        
        mockStorage.Setup(s => s.UploadFileAsync(
                It.IsAny<Stream>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeUrl);
            
        var handler = new UploadAvatarCommandHandler(context, mockStorage.Object);
        var command = new UploadAvatarCommand(
            user.Id, 
            new MemoryStream(new byte[] { 1, 2, 3 }), 
            "image/jpeg", 
            "avatar.jpg");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.AvatarUrl.Should().Be(fakeUrl);
        
        mockStorage.Verify(s => s.UploadFileAsync(
            It.IsAny<Stream>(), 
            "image/jpeg", 
            It.Is<string>(name => name.Contains($"avatars/{user.Id}/") && name.EndsWith(".jpg")), 
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context = new MyDbContext(_options);
        var mockStorage = new Mock<IFileStorageService>();
        var handler = new UploadAvatarCommandHandler(context, mockStorage.Object);
        var command = new UploadAvatarCommand(Guid.NewGuid(), Stream.Null, "image/jpeg", "avatar.jpg");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
