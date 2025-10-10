using LifTools.Models;
using Xunit;

namespace LifTools.Tests;

public class StaticDataProviderTests
{
    [Fact]
    public async Task GetLinesAsync_WithValidLines_ReturnsAllLines()
    {
        // Arrange
        var lines = new[] { "line1", "line2", "line3" };
        var provider = new StaticDataProvider(lines);

        // Act
        var result = await provider.GetLinesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal("line1", result.ElementAt(0));
        Assert.Equal("line2", result.ElementAt(1));
        Assert.Equal("line3", result.ElementAt(2));
    }

    [Fact]
    public async Task GetLinesAsync_WithEmptyLines_FiltersOutEmptyLines()
    {
        // Arrange
        var lines = new[] { "line1", "", "line2", "   ", "line3" };
        var provider = new StaticDataProvider(lines);

        // Act
        var result = await provider.GetLinesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal("line1", result.ElementAt(0));
        Assert.Equal("line2", result.ElementAt(1));
        Assert.Equal("line3", result.ElementAt(2));
    }

    [Fact]
    public void GetLinesAsync_WithNullLines_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StaticDataProvider(null!));
    }

    [Fact]
    public async Task GetLinesAsync_WithEmptyCollection_ReturnsEmptyResult()
    {
        // Arrange
        var lines = new string[0];
        var provider = new StaticDataProvider(lines);

        // Act
        var result = await provider.GetLinesAsync();

        // Assert
        Assert.Empty(result);
    }
}
