using System;
using System.IO;
using FluentAssertions;
using Moq;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Storage;
using Xunit;

namespace MrtOps.Tests.History;

public class OperationHistoryManagerTests : IDisposable
{
    private readonly string _tempHistoryFile;
    private readonly JsonHistoryStorage _storage;
    private readonly OperationHistoryManager _manager;

    public OperationHistoryManagerTests()
    {
        _tempHistoryFile = Path.Combine(Path.GetTempPath(), $"history_{Guid.NewGuid()}.json");
        _storage = new JsonHistoryStorage(_tempHistoryFile);
        _manager = new OperationHistoryManager(_storage);
    }

    public void Dispose()
    {
        if (File.Exists(_tempHistoryFile))
        {
            File.Delete(_tempHistoryFile);
        }
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
    public void UndoLast_ShouldReturnFalse_WhenStackAndStorageAreEmpty()
    {
        bool success = _manager.UndoLast(out string description);

        success.Should().BeFalse();
        description.Should().BeEmpty();
    }

    [Fact]
    public void TryPeekLast_ShouldReturnPreview_WhenStackHasOperation()
    {
        var reversibleMock = new Mock<IReversibleOperation>();
        reversibleMock.Setup(r => r.Execute()).Returns(true);
        reversibleMock.Setup(r => r.Description).Returns("Created test.mrt");
        reversibleMock.Setup(r => r.TargetFilePath).Returns("C:\\test.mrt");
        reversibleMock.Setup(r => r.BackupFilePath).Returns((string?)null);

        _manager.Execute(reversibleMock.Object);

        bool hasPreview = _manager.TryPeekLast(out var preview);

        hasPreview.Should().BeTrue();
        preview.Should().NotBeNull();
        preview!.Description.Should().Be("Created test.mrt");
        preview.TargetFilePath.Should().Be("C:\\test.mrt");
        preview.IsCreation.Should().BeTrue();
    }

    [Fact]
    public void UndoLast_AcrossDifferentInstances_ShouldRevertFromPersistentStorage()
    {
        string targetFile = Path.Combine(Path.GetTempPath(), $"created_{Guid.NewGuid()}.mrt");
        File.WriteAllText(targetFile, "temporary report content");

        var reversibleMock = new Mock<IReversibleOperation>();
        reversibleMock.Setup(r => r.Execute()).Returns(true);
        reversibleMock.Setup(r => r.Description).Returns("Create Report");
        reversibleMock.Setup(r => r.TargetFilePath).Returns(targetFile);
        reversibleMock.Setup(r => r.BackupFilePath).Returns((string?)null);

        // Istanza 1 (es. processo 'gen') esegue l'operazione
        _manager.Execute(reversibleMock.Object);

        // Istanza 2 (es. nuovo processo CLI 'undo') con la stessa persistenza
        var secondManager = new OperationHistoryManager(_storage);

        bool canPeek = secondManager.TryPeekLast(out var preview);
        canPeek.Should().BeTrue();
        preview!.TargetFilePath.Should().Be(targetFile);

        bool undoSuccess = secondManager.UndoLast(out string desc);
        undoSuccess.Should().BeTrue();
        desc.Should().Be("Create Report");

        // Il file target deve essere stato cancellato dall'undo persistente
        File.Exists(targetFile).Should().BeFalse();

        // Secondo tentativo di undo deve fallire perché la history è ora vuota
        bool secondUndo = secondManager.UndoLast(out _);
        secondUndo.Should().BeFalse();
    }
}