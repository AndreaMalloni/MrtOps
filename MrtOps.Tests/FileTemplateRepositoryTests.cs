using FluentAssertions;
using MrtOps.Core.Storage;
using Xunit;

namespace MrtOps.Tests.Storage;

public class FileTemplateRepositoryTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileTemplateRepository _repo;

    public FileTemplateRepositoryTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "MrtOpsTemplates_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        File.WriteAllText(Path.Combine(_testDir, "InvoiceTemplate.mrt"), "dummy");
        File.WriteAllText(Path.Combine(_testDir, "MonthlyReport.mrt"), "dummy");
        File.WriteAllText(Path.Combine(_testDir, "NotATemplate.txt"), "dummy");

        Environment.SetEnvironmentVariable("MRTOPS_TEMPLATES_DIR", _testDir);
        _repo = new FileTemplateRepository();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("MRTOPS_TEMPLATES_DIR", null);
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void GetAvailableTemplates_ShouldReturnOnlyMrtFileNames()
    {
        var templates = _repo.GetAvailableTemplates().ToList();

        templates.Should().HaveCount(2);
        templates.Should().Contain("InvoiceTemplate");
        templates.Should().Contain("MonthlyReport");
        templates.Should().NotContain("NotATemplate");
    }

    [Fact]
    public void GetTemplateFilePath_ShouldReturnFullPath_WhenFileExists()
    {
        var result = _repo.GetTemplateFilePath("InvoiceTemplate");

        result.Should().Be(Path.Combine(_testDir, "InvoiceTemplate.mrt"));
    }

    [Fact]
    public void GetTemplateFilePath_ShouldThrowFileNotFound_WhenFileDoesNotExist()
    {
        Action act = () => _repo.GetTemplateFilePath("NonExistent");

        act.Should().Throw<FileNotFoundException>();
    }
}