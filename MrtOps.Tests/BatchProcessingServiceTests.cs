using Moq;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using Xunit;

namespace MrtOps.Tests.Services;

public class BatchProcessingServiceTests : IDisposable
{
    private readonly Mock<IReportEngine> _engineMock;
    private readonly Mock<ILocalizationService> _locMock;
    private readonly OperationHistoryManager _history;
    private readonly BatchProcessingService _service;
    private readonly string _testDir;

    public BatchProcessingServiceTests()
    {
        _engineMock = new Mock<IReportEngine>();
        _locMock = new Mock<ILocalizationService>();
        _history = new OperationHistoryManager();

        _locMock.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Log");

        _service = new BatchProcessingService(_history, _engineMock.Object, _locMock.Object);

        _testDir = Path.Combine(Path.GetTempPath(), "MrtOpsTestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        File.WriteAllText(Path.Combine(_testDir, "file1.mrt"), "data");
        File.WriteAllText(Path.Combine(_testDir, "file2.mrt"), "data");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void ProcessVariableOperation_Add_ShouldProcessAllFiles_WhenDryRunIsFalse()
    {
        _service.ProcessVariableOperation(new[] { _testDir }, "add", "Cat", "Var", null, "string", "Value", false);
        _engineMock.Verify(e => e.AddVariableToReport(It.IsAny<string>(), "Cat", "Var", "string", "Value"), Times.Exactly(2));
    }

    [Fact]
    public void ProcessVariableOperation_Add_ShouldNotCallEngine_WhenDryRunIsTrue()
    {
        _service.ProcessVariableOperation(new[] { _testDir }, "add", "Cat", "Var", null, "string", "Value", true);
        _engineMock.Verify(e => e.AddVariableToReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ProcessVariableOperation_Remove_ShouldCallEngine()
    {
        _service.ProcessVariableOperation(new[] { _testDir }, "remove", "Cat", "Var", null, null, null, false);
        _engineMock.Verify(e => e.RemoveVariableFromReport(It.IsAny<string>(), "Var"), Times.Exactly(2));
    }

    [Fact]
    public void ProcessVariableOperation_Modify_ShouldCallEngine()
    {
        _service.ProcessVariableOperation(new[] { _testDir }, "modify", "Cat", "Var", "NewVar", "int", "Expression", false);
        _engineMock.Verify(e => e.ModifyVariableInReport(It.IsAny<string>(), "Var", "NewVar", "Cat", "int", "Expression"), Times.Exactly(2));
    }

    [Fact]
    public void ProcessVariableOperation_SingleFile_ShouldProcessOnlyOneFile()
    {
        var singleFile = Path.Combine(_testDir, "file1.mrt");
        _service.ProcessVariableOperation(new[] { singleFile }, "add", "Cat", "Var", null, "string", "Value", false);
        _engineMock.Verify(e => e.AddVariableToReport(singleFile, "Cat", "Var", "string", "Value"), Times.Once);
    }
}