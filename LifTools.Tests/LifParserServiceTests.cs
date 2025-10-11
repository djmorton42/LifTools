using LifTools.Models;
using Xunit;

namespace LifTools.Tests;

public class LifParserServiceTests
{
    [Fact]
    public async Task ParseRaceAsync_WithValidLifData_ParsesCorrectly()
    {
        // Arrange
        var lifData = new[]
        {
            "18B,1,1,Open Women B (1000 111M) Final,,,,,,,14:25:33.4364",
            "1,392,5,Smith,Jane,Ottawa,1:55.893,,1:55.893,,\"3.106 (3.106),1:09.119 (1:06.013),1:55.893 (46.774),2:20.991 (25.099)\",14:25:33.437,,,,1:55.893,1:55.893,",
            "2,177,2,Johnson,Alice,Ottawa,1:57.058,,1.165,,\"18.489 (18.489),31.286 (12.797),43.880 (12.594),56.948 (13.068),1:09.353 (12.405),1:21.440 (12.087),1:46.439 (24.999),1:57.058 (10.619),1:59.394 (2.337)\",14:25:33.437,,,,1.165,1.165,"
        };
        var dataProvider = new StaticDataProvider(lifData);
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert
        Assert.NotNull(race);
        Assert.NotNull(race.RaceInfo);
        Assert.Equal("18B", race.RaceInfo.RaceNumber);
        Assert.Equal(1, race.RaceInfo.Heat);
        Assert.Equal(1, race.RaceInfo.Round);
        Assert.Equal("Open Women B (1000 111M) Final", race.RaceInfo.EventName);
        Assert.Equal("14:25:33.4364", race.RaceInfo.StartTime);
        
        Assert.Equal(2, race.Racers.Count);
        
        // First racer
        var firstRacer = race.Racers[0];
        Assert.Equal("1", firstRacer.Position.Value);
        Assert.Equal(392, firstRacer.RacerId);
        Assert.Equal(5, firstRacer.LineNumber);
        Assert.Equal("Smith", firstRacer.LastName);
        Assert.Equal("Jane", firstRacer.FirstName);
        Assert.Equal("Ottawa", firstRacer.Affiliation);
        Assert.Equal("115.893", firstRacer.FinishTimeRaw);
        Assert.Equal("1:55.893", firstRacer.FinishTimeFormatted);
        Assert.Equal("1:55.893", firstRacer.DeltaTime);
        Assert.Equal("3.106 (3.106),1:09.119 (1:06.013),1:55.893 (46.774),2:20.991 (25.099)", firstRacer.Splits);
        
        // Second racer
        var secondRacer = race.Racers[1];
        Assert.Equal("2", secondRacer.Position.Value);
        Assert.Equal(177, secondRacer.RacerId);
        Assert.Equal(2, secondRacer.LineNumber);
        Assert.Equal("Johnson", secondRacer.LastName);
        Assert.Equal("Alice", secondRacer.FirstName);
        Assert.Equal("Ottawa", secondRacer.Affiliation);
        Assert.Equal("117.058", secondRacer.FinishTimeRaw);
        Assert.Equal("1.165", secondRacer.DeltaTime);
    }

    [Fact]
    public async Task ParseRaceAsync_WithEmptyData_ThrowsInvalidOperationException()
    {
        // Arrange
        var dataProvider = new StaticDataProvider(new string[0]);
        var parser = new LifParserService(dataProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => parser.ParseRaceAsync());
    }

    [Fact]
    public async Task ParseRaceAsync_WithOnlyRaceInfo_ParsesCorrectly()
    {
        // Arrange
        var lifData = new[]
        {
            "18B,1,1,Open Women B (1000 111M) Final,,,,,,,14:25:33.4364"
        };
        var dataProvider = new StaticDataProvider(lifData);
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert
        Assert.NotNull(race);
        Assert.NotNull(race.RaceInfo);
        Assert.Equal("18B", race.RaceInfo.RaceNumber);
        Assert.Empty(race.Racers);
    }

    [Fact]
    public async Task ParseRaceAsync_WithUnusedFields_PreservesUnusedFields()
    {
        // Arrange
        var lifData = new[]
        {
            "18B,1,1,Open Women B (1000 111M) Final,unused1,unused2,unused3,unused4,unused5,,14:25:33.4364",
            "1,392,5,Smith,Jane,Ottawa,1:55.893,,1:55.893,,\"3.106 (3.106)\",14:25:33.437,unused1,unused2,unused3,unused4,unused5"
        };
        var dataProvider = new StaticDataProvider(lifData);
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert
        Assert.Equal("unused1", race.RaceInfo.Unused1);
        Assert.Equal("unused2", race.RaceInfo.Unused2);
        Assert.Equal("unused3", race.RaceInfo.Unused3);
        Assert.Equal("unused4", race.RaceInfo.Unused4);
        Assert.Equal("unused5", race.RaceInfo.Unused5);
        
        Assert.Equal("unused1", race.Racers[0].Unused1);
        Assert.Equal("unused2", race.Racers[0].Unused2);
        Assert.Equal("unused3", race.Racers[0].Unused3);
        Assert.Equal("unused4", race.Racers[0].Unused4);
        Assert.Equal("unused5", race.Racers[0].Unused5);
    }

    [Fact]
    public async Task ParseRaceAsync_WithMissingFields_HandlesGracefully()
    {
        // Arrange
        var lifData = new[]
        {
            "18B,1,1,Open Women B Final",
            "1,392,5,Smith,Jane,Ottawa,1:55.893"
        };
        var dataProvider = new StaticDataProvider(lifData);
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert
        Assert.NotNull(race);
        Assert.Equal("18B", race.RaceInfo.RaceNumber);
        Assert.Equal(1, race.RaceInfo.Heat);
        Assert.Equal(1, race.RaceInfo.Round);
        Assert.Equal("Open Women B Final", race.RaceInfo.EventName);
        
        Assert.Single(race.Racers);
        Assert.Equal("1", race.Racers[0].Position.Value);
        Assert.Equal(392, race.Racers[0].RacerId);
        Assert.Equal(5, race.Racers[0].LineNumber);
        Assert.Equal("Smith", race.Racers[0].LastName);
        Assert.Equal("Jane", race.Racers[0].FirstName);
        Assert.Equal("Ottawa", race.Racers[0].Affiliation);
        Assert.Equal("115.893", race.Racers[0].FinishTimeRaw);
    }

    [Fact]
    public void LifParserService_WithNullDataProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LifParserService(null!));
    }

    [Theory]
    [InlineData("18B,1,1,Event Name,,,,,,,14:25:33.4364", "18B", 1, 1, "Event Name", "14:25:33.4364")]
    [InlineData("19C,2,3,Another Event,unused1,unused2,unused3,unused4,unused5,,15:30:45.1234", "19C", 2, 3, "Another Event", "15:30:45.1234")]
    [InlineData("20D,5,10,Test Event,,,,,,,16:45:12.0000", "20D", 5, 10, "Test Event", "16:45:12.0000")]
    public async Task ParseRaceInfo_WithVariousInputs_ParsesCorrectly(
        string raceInfoLine, 
        string expectedRaceNumber, 
        int expectedHeat, 
        int expectedRound, 
        string expectedEventName, 
        string expectedStartTime)
    {
        // Arrange
        var dataProvider = new StaticDataProvider(new[] { raceInfoLine });
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert
        Assert.Equal(expectedRaceNumber, race.RaceInfo.RaceNumber);
        Assert.Equal(expectedHeat, race.RaceInfo.Heat);
        Assert.Equal(expectedRound, race.RaceInfo.Round);
        Assert.Equal(expectedEventName, race.RaceInfo.EventName);
        Assert.Equal(expectedStartTime, race.RaceInfo.StartTime);
    }
}
