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
    public void ProcessFolderAddVariable_ShouldProcessAllFiles_WhenDryRunIsFalse()
    {
        _service.ProcessFolderAddVariable(_testDir, "Cat", "Var", false);
        _engineMock.Verify(e => e.AddVariableToReport(It.IsAny<string>(), "Cat", "Var"), Times.Exactly(2));
    }

    [Fact]
    public void ProcessFolderAddVariable_ShouldNotCallEngine_WhenDryRunIsTrue()
    {
        _service.ProcessFolderAddVariable(_testDir, "Cat", "Var", true);
        _engineMock.Verify(e => e.AddVariableToReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}