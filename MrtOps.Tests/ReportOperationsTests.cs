using FluentAssertions;
using Moq;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Models;
using MrtOps.Core.Operations;
using Xunit;

namespace MrtOps.Tests.Operations;

public class ReportOperationsTests : IDisposable
{
    private readonly Mock<IReportEngine> _engineMock;
    private readonly Mock<ILocalizationService> _locMock;
    private readonly Mock<ITemplateRepository> _templateRepoMock;
    private readonly string _testFile = "test_report.mrt";

    public ReportOperationsTests()
    {
        _engineMock = new Mock<IReportEngine>();
        _locMock = new Mock<ILocalizationService>();
        _locMock.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Test Description");
        _templateRepoMock = new Mock<ITemplateRepository>();

        File.WriteAllText(_testFile, "dummy content");
    }

    public void Dispose()
    {
        if (File.Exists(_testFile)) File.Delete(_testFile);
        if (File.Exists(_testFile + ".bak")) File.Delete(_testFile + ".bak");
    }

    [Fact]
    public void AddVariableOperation_ShouldCallEngineAndCreateBackup()
    {
        var operation = new AddVariableOperation(_engineMock.Object, _locMock.Object, _testFile, "TestCategory", "TestVar");

        bool result = operation.Execute();

        result.Should().BeTrue();
        File.Exists(_testFile + ".bak").Should().BeTrue();
        _engineMock.Verify(e => e.AddVariableToReport(_testFile, "TestCategory", "TestVar"), Times.Once);
    }

    [Fact]
    public void ApplyStyleOperation_ShouldCallEngineAndCreateBackup()
    {
        var styleFile = "test.sts";
        var operation = new ApplyStyleOperation(_engineMock.Object, _locMock.Object, _testFile, styleFile);

        bool result = operation.Execute();

        result.Should().BeTrue();
        File.Exists(_testFile + ".bak").Should().BeTrue();
        _engineMock.Verify(e => e.ApplyStyleToReport(_testFile, styleFile), Times.Once);
    }

    [Fact]
    public void SyncStringsOperation_ShouldCallEngineAndCreateBackup()
    {
        var dictionary = new Dictionary<string, Dictionary<string, string>>
        {
            { "en", new Dictionary<string, string> { { "Key", "Val" } } }
        };
        var operation = new SyncStringsOperation(_engineMock.Object, _locMock.Object, _testFile, dictionary);

        bool result = operation.Execute();

        result.Should().BeTrue();
        File.Exists(_testFile + ".bak").Should().BeTrue();
        _engineMock.Verify(e => e.SyncGlobalizationStrings(_testFile, dictionary), Times.Once);
    }

    [Fact]
    public void CreateReportOperation_Execute_ShouldCallGenerateReport()
    {
        var metadata = new ReportMetadata(
            Name: "Test",
            Alias: "TestAlias",
            Description: "Descrizione di test",
            OutputPath: _testFile,
            TemplateName: "BaseTemplate"
        );
        var template = new ReportTemplateDef { Author = "Tester" };
        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, _templateRepoMock.Object, metadata);

        bool result = operation.Execute();

        result.Should().BeTrue();
        _engineMock.Verify(e => e.GenerateReport(metadata, template), Times.Once);
    }

    [Fact]
    public void CreateReportOperation_Execute_ShouldCopyTemplateAndUpdateMetadata()
    {
        var templateRepoMock = new Mock<ITemplateRepository>();
        var sourceTemplate = "dummy_template.mrt";
        var destinationFile = "new_report.mrt";

        File.WriteAllText(sourceTemplate, "template content");

        templateRepoMock.Setup(r => r.GetTemplateFilePath("BaseTemplate")).Returns(sourceTemplate);

        var metadata = new ReportMetadata(
            Name: "TestReport",
            Alias: "Test Alias",
            Description: "Desc",
            OutputPath: destinationFile,
            TemplateName: "BaseTemplate"
        );

        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, templateRepoMock.Object, metadata);

        bool result = operation.Execute();

        result.Should().BeTrue();
        File.Exists(destinationFile).Should().BeTrue(); 
        _engineMock.Verify(e => e.UpdateReportMetadata(destinationFile, metadata), Times.Once);

        if (File.Exists(sourceTemplate)) File.Delete(sourceTemplate);
        if (File.Exists(destinationFile)) File.Delete(destinationFile);
    }
}