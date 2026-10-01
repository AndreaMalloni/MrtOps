using System.IO;
using FluentAssertions;
using MrtOps.Core;
using Xunit;

namespace MrtOps.Tests;

public class PathResolutionTests
{
    [Fact]
    public void Resolve_WithRelativeDirectoryAndName_ShouldAppendNameToDirectory()
    {
        var (fullPath, name) = ReportPathResolver.Resolve("./", "test");

        name.Should().Be("test");
        fullPath.Should().EndWith($"{Path.DirectorySeparatorChar}test.mrt");
        Path.GetFileName(fullPath).Should().Be("test.mrt");
    }

    [Fact]
    public void Resolve_WithBackslashDirectoryAndName_ShouldAppendNameToDirectory()
    {
        var (fullPath, name) = ReportPathResolver.Resolve(".\\", "invoice");

        name.Should().Be("invoice");
        Path.GetFileName(fullPath).Should().Be("invoice.mrt");
    }

    [Fact]
    public void Resolve_WithDotAndName_ShouldAppendNameToCurrentDirectory()
    {
        var (fullPath, name) = ReportPathResolver.Resolve(".", "my_report");

        name.Should().Be("my_report");
        Path.GetFileName(fullPath).Should().Be("my_report.mrt");
    }

    [Fact]
    public void Resolve_WithMrtFileName_ShouldKeepPathAndInferNameIfOmitted()
    {
        var (fullPath, name) = ReportPathResolver.Resolve("sales.mrt", null);

        name.Should().Be("sales");
        Path.GetFileName(fullPath).Should().Be("sales.mrt");
    }

    [Fact]
    public void Resolve_WithMrtFileNameAndExplicitName_ShouldUseExplicitName()
    {
        var (fullPath, name) = ReportPathResolver.Resolve("output_file.mrt", "CustomName");

        name.Should().Be("CustomName");
        Path.GetFileName(fullPath).Should().Be("output_file.mrt");
    }

    [Fact]
    public void Resolve_WithFileNameWithoutExtension_ShouldAddMrtExtension()
    {
        var (fullPath, name) = ReportPathResolver.Resolve("my_report", "test");

        name.Should().Be("test");
        Path.GetFileName(fullPath).Should().Be("my_report.mrt");
    }

    [Fact]
    public void Resolve_WithNullPathAndNullName_ShouldDefaultToCurrentDirectoryAndReport()
    {
        var (fullPath, name) = ReportPathResolver.Resolve(null, null);

        name.Should().Be("Report");
        Path.GetFileName(fullPath).Should().Be("Report.mrt");
    }
}
