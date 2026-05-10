using FluentAssertions;
using Moq;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using Xunit;

namespace MrtOps.Tests.History;

public class OperationHistoryManagerTests
{
    private readonly OperationHistoryManager _manager;

    public OperationHistoryManagerTests()
    {
        _manager = new OperationHistoryManager();
    }

    [Fact]
    public void Execute_ShouldReturnTrue_WhenOperationSucceeds()
    {
        var operationMock = new Mock<IOperation>();
        operationMock.Setup(o => o.Execute()).Returns(true);

        bool result = _manager.Execute(operationMock.Object);

        result.Should().BeTrue();
        operationMock.Verify(o => o.Execute(), Times.Once);
    }

    [Fact]
    public void UndoLast_ShouldCallUndoOnLastOperation_WhenStackIsNotEmpty()
    {
        var operation1Mock = new Mock<IOperation>();
        var operation2Mock = new Mock<IOperation>();

        operation1Mock.Setup(o => o.Execute()).Returns(true);
        operation2Mock.Setup(o => o.Execute()).Returns(true);
        operation2Mock.Setup(o => o.Undo()).Returns(true);
        operation2Mock.Setup(o => o.Description).Returns("Op 2");

        _manager.Execute(operation1Mock.Object);
        _manager.Execute(operation2Mock.Object);

        bool success = _manager.UndoLast(out string description);

        success.Should().BeTrue();
        description.Should().Be("Op 2");
        operation2Mock.Verify(o => o.Undo(), Times.Once);
        operation1Mock.Verify(o => o.Undo(), Times.Never);
    }

    [Fact]
    public void UndoLast_ShouldReturnFalse_WhenStackIsEmpty()
    {
        bool success = _manager.UndoLast(out string description);

        success.Should().BeFalse();
        description.Should().BeEmpty();
    }
}