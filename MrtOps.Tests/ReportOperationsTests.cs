using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<ILogger<CreateReportOperation>> _loggerMock;
    private readonly string _testFile = "test_report.mrt";

    public ReportOperationsTests()
    {
        _engineMock = new Mock<IReportEngine>();
        _locMock = new Mock<ILocalizationService>();
        _locMock.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Test Description");
        _templateRepoMock = new Mock<ITemplateRepository>();
        _loggerMock = new Mock<ILogger<CreateReportOperation>>();

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
    public void CreateReportOperation_Execute_WhenNoTemplate_ShouldCallCreateEmptyReportAndUpdateMetadata()
    {
        var metadata = new ReportMetadata(
            Name: "Test",
            Alias: "TestAlias",
            Description: "Descrizione di test",
            OutputPath: _testFile,
            TemplateName: string.Empty
        );

        _engineMock.Setup(e => e.CreateEmptyReport(_testFile)).Returns(true);
        _engineMock.Setup(e => e.UpdateReportMetadata(_testFile, metadata)).Returns(true);

        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, _templateRepoMock.Object, metadata, _loggerMock.Object);

        bool result = operation.Execute();

        result.Should().BeTrue();
        _engineMock.Verify(e => e.CreateEmptyReport(_testFile), Times.Once);
        _engineMock.Verify(e => e.UpdateReportMetadata(_testFile, metadata), Times.Once);
    }

    [Fact]
    public void CreateReportOperation_Execute_WhenEngineFails_ShouldReturnFalse()
    {
        var metadata = new ReportMetadata(
            Name: "TestFail",
            Alias: "TestFail",
            Description: "Fail",
            OutputPath: _testFile,
            TemplateName: string.Empty
        );

        _engineMock.Setup(e => e.CreateEmptyReport(_testFile)).Returns(false);

        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, _templateRepoMock.Object, metadata, _loggerMock.Object);

        bool result = operation.Execute();

        result.Should().BeFalse();
        _engineMock.Verify(e => e.UpdateReportMetadata(It.IsAny<string>(), It.IsAny<ReportMetadata>()), Times.Never);
    }

    [Fact]
    public void CreateReportOperation_Execute_ShouldCopyTemplateAndUpdateMetadata()
    {
        var templateRepoMock = new Mock<ITemplateRepository>();
        var sourceTemplate = "dummy_template.mrt";
        var destinationFile = "new_report.mrt";

        File.WriteAllText(sourceTemplate, "template content");

        templateRepoMock.Setup(r => r.GetTemplateFilePath("BaseTemplate")).Returns(sourceTemplate);
        _engineMock.Setup(e => e.UpdateReportMetadata(destinationFile, It.IsAny<ReportMetadata>())).Returns(true);

        var metadata = new ReportMetadata(
            Name: "TestReport",
            Alias: "Test Alias",
            Description: "Desc",
            OutputPath: destinationFile,
            TemplateName: "BaseTemplate"
        );

        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, templateRepoMock.Object, metadata, _loggerMock.Object);

        bool result = operation.Execute();

        result.Should().BeTrue();
        File.Exists(destinationFile).Should().BeTrue();
        _engineMock.Verify(e => e.UpdateReportMetadata(destinationFile, metadata), Times.Once);

        if (File.Exists(sourceTemplate)) File.Delete(sourceTemplate);
        if (File.Exists(destinationFile)) File.Delete(destinationFile);
    }

    [Fact]
    public void CreateReportOperation_Undo_ShouldDeleteCreatedFile()
    {
        var createdFile = "to_delete.mrt";
        File.WriteAllText(createdFile, "content");

        var metadata = new ReportMetadata("DeleteMe", "DeleteMe", "", createdFile, "");
        var operation = new CreateReportOperation(_engineMock.Object, _locMock.Object, _templateRepoMock.Object, metadata, _loggerMock.Object);

        bool undoResult = operation.Undo();

        undoResult.Should().BeTrue();
        File.Exists(createdFile).Should().BeFalse();
    }
}