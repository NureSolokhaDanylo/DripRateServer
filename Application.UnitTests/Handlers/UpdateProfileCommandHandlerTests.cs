using Application.Commands;
using Application.Handlers.Commands;
using Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Handlers;

public sealed class UpdateProfileCommandHandlerTests
{
    private readonly DbContextOptions<MyDbContext> _options;

    public UpdateProfileCommandHandlerTests()
    {
        _options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Handle_ShouldUpdateBio_WhenUserExists()
    {
        // Arrange
        await using var context = new MyDbContext(_options);
        var user = new User("test@test.com", "testuser");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateProfileCommandHandler(context);
        var newBio = "This is my new fashion bio.";
        var command = new UpdateProfileCommand(user.Id, newBio);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Bio.Should().Be(newBio);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context = new MyDbContext(_options);
        var handler = new UpdateProfileCommandHandler(context);
        var command = new UpdateProfileCommand(Guid.NewGuid(), "Bio");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
