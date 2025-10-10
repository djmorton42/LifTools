using LifTools.Models;
using Xunit;

namespace LifTools.Tests;

public class LifFileOutputTest
{
    [Fact]
    public async Task ParseAndOutputLifFile_ShowsParsedContents()
    {
        // Arrange
        var filePath = Path.Combine("..", "..", "..", "..", "etc", "18B-1-01.lif");
        var dataProvider = new FileDataProvider(filePath);
        var parser = new LifParserService(dataProvider);

        // Act
        var race = await parser.ParseRaceAsync();

        // Assert & Output
        Console.WriteLine("=== LIF File Parsed Successfully ===");
        Console.WriteLine();
        Console.WriteLine(race.ToString());
        Console.WriteLine();
        
        // Additional detailed output
        Console.WriteLine("=== Detailed Race Information ===");
        Console.WriteLine($"Race Number: {race.RaceInfo.RaceNumber}");
        Console.WriteLine($"Heat: {race.RaceInfo.Heat}");
        Console.WriteLine($"Round: {race.RaceInfo.Round}");
        Console.WriteLine($"Event: {race.RaceInfo.EventName}");
        Console.WriteLine($"Start Time: {race.RaceInfo.StartTime}");
        Console.WriteLine();
        
        Console.WriteLine("=== Racer Details ===");
        for (int i = 0; i < race.Racers.Count; i++)
        {
            var racer = race.Racers[i];
            Console.WriteLine($"Position {racer.Position}:");
            Console.WriteLine($"  Name: {racer.FirstName} {racer.LastName}");
            Console.WriteLine($"  Affiliation: {racer.Affiliation}");
            Console.WriteLine($"  Finish Time: {racer.FinishTimeRaw}");
            Console.WriteLine($"  Delta Time: {racer.DeltaTime}");
            Console.WriteLine($"  Lane Number: {racer.LineNumber}");
            Console.WriteLine($"  Racer ID: {racer.RacerId}");
            Console.WriteLine($"  Splits: {racer.Splits}");
            Console.WriteLine();
        }
        
        // Verify the parsing worked correctly
        Assert.NotNull(race);
        Assert.NotNull(race.RaceInfo);
        Assert.Equal("18B", race.RaceInfo.RaceNumber);
        Assert.Equal(1, race.RaceInfo.Heat);
        Assert.Equal(1, race.RaceInfo.Round);
        Assert.Equal("Open Women B (1000 111M) Final", race.RaceInfo.EventName);
        Assert.Equal("14:25:33.4364", race.RaceInfo.StartTime);
        Assert.Equal(6, race.Racers.Count);
        
        // Verify first and last racers
        var firstRacer = race.Racers[0];
        Assert.Equal("1", firstRacer.Position.Value);
        Assert.Equal("Sofia", firstRacer.FirstName);
        Assert.Equal("King", firstRacer.LastName);
        Assert.Equal("Ottawa", firstRacer.Affiliation);
        Assert.Equal("1:55.893", firstRacer.FinishTimeRaw);
        
        var lastRacer = race.Racers[5];
        Assert.Equal("6", lastRacer.Position.Value);
        Assert.Equal("Evelyn", lastRacer.FirstName);
        Assert.Equal("Flores", lastRacer.LastName);
        Assert.Equal("Kingston", lastRacer.Affiliation);
        Assert.Equal("1:59.225", lastRacer.FinishTimeRaw);
    }
}
