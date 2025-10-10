using LifTools.Models;
using System.Text;
using Xunit;

namespace LifTools.Tests;

public class FileDataProviderTests
{
    [Fact]
    public async Task GetLinesAsync_WithValidFile_ReturnsAllLines()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var testLines = new[] { "line1", "line2", "line3" };
            File.WriteAllLines(tempFile, testLines, Encoding.UTF8);
            var provider = new FileDataProvider(tempFile);

            // Act
            var result = await provider.GetLinesAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal("line1", result.ElementAt(0));
            Assert.Equal("line2", result.ElementAt(1));
            Assert.Equal("line3", result.ElementAt(2));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetLinesAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.lif");
        var provider = new FileDataProvider(nonExistentFile);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.GetLinesAsync());
    }

    [Fact]
    public void FileDataProvider_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileDataProvider(null!));
    }

    [Fact]
    public async Task GetLinesAsync_WithEmptyFile_ReturnsEmptyResult()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "", Encoding.UTF8);
            var provider = new FileDataProvider(tempFile);

            // Act
            var result = await provider.GetLinesAsync();

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
