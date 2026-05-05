using Application.Commands;
using Application.Handlers.Commands;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Application.UnitTests.Handlers.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IDeletionService> _deletionServiceMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userManagerMock = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
        _deletionServiceMock = new Mock<IDeletionService>();
        _handler = new DeleteUserCommandHandler(_contextMock.Object, _userManagerMock.Object, _deletionServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsModerator_ReturnsCannotDeleteModerator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@test.com", "test");
        var command = new DeleteUserCommand(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Moderator"))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.CannotDeleteModeratorCode, result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WhenUserIsRegularUser_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@test.com", "test");
        var command = new DeleteUserCommand(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Moderator"))
            .ReturnsAsync(false);
        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
        _deletionServiceMock.Verify(x => x.DeleteUserContentAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
