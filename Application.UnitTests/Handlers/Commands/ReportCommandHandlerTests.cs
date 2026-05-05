using Application.Commands;
using Application.Handlers.Commands;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Application.UnitTests.Handlers.Commands;

public sealed class ReportCommandHandlerTests
{
    [Fact]
    public async Task Assign_WhenAlreadyAssignedToOther_ReturnsConflict()
    {
        // Arrange
        var moderator1Id = Guid.NewGuid();
        var moderator2Id = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var targetType = ReportTargetType.Publication;

        var report = new Report(Guid.NewGuid(), targetType, targetId, ReportCategory.Spam, "test");
        report.AssignTo(moderator1Id);

        var reportsMock = new List<Report> { report }.BuildMockDbSet();

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(x => x.Reports).Returns(reportsMock.Object);

        var handler = new AssignReportedEntityCommandHandler(dbContextMock.Object);
        var command = new AssignReportedEntityCommand(moderator2Id, targetType, targetId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ReportErrors.AlreadyAssignedCode, result.FirstError.Code);
    }

    [Fact]
    public async Task Assign_WhenNotAssigned_AssignsToModerator()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var targetType = ReportTargetType.Publication;

        var report = new Report(Guid.NewGuid(), targetType, targetId, ReportCategory.Spam, "test");
        var reportsList = new List<Report> { report };
        var reportsMock = reportsList.BuildMockDbSet();

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(x => x.Reports).Returns(reportsMock.Object);

        var handler = new AssignReportedEntityCommandHandler(dbContextMock.Object);
        var command = new AssignReportedEntityCommand(moderatorId, targetType, targetId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(moderatorId, report.AssignedToUserId);
        Assert.Equal(ReportStatus.InReview, report.Status);
        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
