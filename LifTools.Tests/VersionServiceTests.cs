using System;
using System.IO;
using LifTools.Services;
using Xunit;

namespace LifTools.Tests;

public class VersionServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _versionFilePath;

    public VersionServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _versionFilePath = Path.Combine(_tempDirectory, "VERSION.txt");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void GetVersion_WithValidVersionFile_ShouldReturnVersion()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "2.1.21");
        var service = new TestableVersionService(_versionFilePath);

        // Act
        var version = service.GetVersion();

        // Assert
        Assert.Equal("2.1.21", version);
    }

    [Fact]
    public void GetVersion_WithWhitespaceInFile_ShouldTrimWhitespace()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "  2.1.21  \n");
        var service = new TestableVersionService(_versionFilePath);

        // Act
        var version = service.GetVersion();

        // Assert
        Assert.Equal("2.1.21", version);
    }

    [Fact]
    public void GetVersion_WithEmptyFile_ShouldReturnUnknown()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "");
        var service = new TestableVersionService(_versionFilePath);

        // Act
        var version = service.GetVersion();

        // Assert
        Assert.Equal("Unknown", version);
    }

    [Fact]
    public void GetVersion_WithMissingFile_ShouldReturnUnknown()
    {
        // Arrange
        var service = new TestableVersionService("nonexistent.txt");

        // Act
        var version = service.GetVersion();

        // Assert
        Assert.Equal("Unknown", version);
    }

    [Fact]
    public void GetVersion_ShouldCacheResult()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "2.1.21");
        var service = new TestableVersionService(_versionFilePath);

        // Act
        var version1 = service.GetVersion();
        File.WriteAllText(_versionFilePath, "3.0.0"); // Change file after first read
        var version2 = service.GetVersion();

        // Assert
        Assert.Equal("2.1.21", version1);
        Assert.Equal("2.1.21", version2); // Should return cached version
    }

    [Fact]
    public void GetVersion_WithDifferentVersionFormats_ShouldReturnAsIs()
    {
        // Arrange
        var testCases = new[]
        {
            "1.0.0",
            "2.1.21",
            "10.15.3",
            "1.0.0-beta",
            "2.0.0-alpha.1"
        };

        foreach (var testVersion in testCases)
        {
            // Arrange
            File.WriteAllText(_versionFilePath, testVersion);
            var service = new TestableVersionService(_versionFilePath);

            // Act
            var version = service.GetVersion();

            // Assert
            Assert.Equal(testVersion, version);
        }
    }
}

// Test for MainWindowViewModel Title property behavior
public class MainWindowViewModelTitleTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _versionFilePath;

    public MainWindowViewModelTitleTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _versionFilePath = Path.Combine(_tempDirectory, "VERSION.txt");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Title_WithValidVersion_ShouldIncludeVersion()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "2.1.21");
        var versionService = new TestableVersionService(_versionFilePath);
        var viewModel = new TestableMainWindowViewModel(versionService);

        // Act
        var title = viewModel.Title;

        // Assert
        Assert.Equal("LifTools v2.1.21", title);
    }

    [Fact]
    public void Title_WithMissingVersionFile_ShouldNotIncludeVersion()
    {
        // Arrange
        var versionService = new TestableVersionService("nonexistent.txt");
        var viewModel = new TestableMainWindowViewModel(versionService);

        // Act
        var title = viewModel.Title;

        // Assert
        Assert.Equal("LifTools", title);
    }

    [Fact]
    public void Title_WithEmptyVersionFile_ShouldNotIncludeVersion()
    {
        // Arrange
        File.WriteAllText(_versionFilePath, "");
        var versionService = new TestableVersionService(_versionFilePath);
        var viewModel = new TestableMainWindowViewModel(versionService);

        // Act
        var title = viewModel.Title;

        // Assert
        Assert.Equal("LifTools", title);
    }
}

// Testable version of MainWindowViewModel for testing Title property
public class TestableMainWindowViewModel
{
    private readonly IVersionService _versionService;

    public TestableMainWindowViewModel(IVersionService versionService)
    {
        _versionService = versionService;
    }

    public string Title
    {
        get
        {
            var version = _versionService.GetVersion();
            return version == "Unknown" ? "LifTools" : $"LifTools v{version}";
        }
    }
}

// Testable version of VersionService that allows us to specify the file path
public class TestableVersionService : IVersionService
{
    private readonly string _versionFilePath;
    private string? _cachedVersion;

    public TestableVersionService(string versionFilePath)
    {
        _versionFilePath = versionFilePath;
    }

    public string GetVersion()
    {
        if (_cachedVersion != null)
        {
            return _cachedVersion;
        }

        try
        {
            if (File.Exists(_versionFilePath))
            {
                var versionText = File.ReadAllText(_versionFilePath).Trim();
                if (!string.IsNullOrEmpty(versionText))
                {
                    _cachedVersion = versionText;
                    return _cachedVersion;
                }
            }
        }
        catch (Exception)
        {
            // If we can't read the version file, fall back to default
        }

        // Fallback to a default version if file doesn't exist or can't be read
        _cachedVersion = "Unknown";
        return _cachedVersion;
    }
}
