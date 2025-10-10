using LifTools.Models;
using Xunit;

namespace LifTools.Tests;

public class CalculatorServiceTests
{
    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        // Arrange
        var calculator = new CalculatorService();
        int a = 5;
        int b = 3;
        int expected = 8;

        // Act
        int result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 2)]
    [InlineData(-1, 1, 0)]
    [InlineData(-5, -3, -8)]
    [InlineData(100, 200, 300)]
    public void Add_ShouldReturnCorrectSumForVariousInputs(int a, int b, int expected)
    {
        // Arrange
        var calculator = new CalculatorService();

        // Act
        int result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }
}
