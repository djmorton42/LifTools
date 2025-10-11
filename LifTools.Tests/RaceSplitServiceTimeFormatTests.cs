using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LifTools.Models;
using LifTools.Services;
using Xunit;

namespace LifTools.Tests;

public class RaceSplitServiceTimeFormatTests : IDisposable
{
    private readonly RaceSplitService _service;
    private readonly string _tempDirectory;

    public RaceSplitServiceTimeFormatTests()
    {
        _service = new RaceSplitService();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task SplitRaceAsync_WithRealLifData_ShouldFormatTimesCorrectly()
    {
        // Arrange - Create a race with times that would be parsed as raw seconds
        var race = new Race
        {
            RaceInfo = new RaceInfo
            {
                RaceNumber = "18B",
                Heat = 1,
                Round = 1,
                EventName = "Test Event",
                StartTime = "14:25:33.4364"
            }
        };

        // Create racers with raw seconds times (as they would be after parsing)
        race.Racers.Add(new Racer
        {
            Position = new Position("1"),
            RacerId = 392,
            LineNumber = 5,
            FirstName = "Sofia",
            LastName = "King",
            Affiliation = "Ottawa",
            FinishTimeRaw = "115.893", // 1:55.893 in raw seconds
            License = "L001"
        });

        race.Racers.Add(new Racer
        {
            Position = new Position("2"),
            RacerId = 177,
            LineNumber = 2,
            FirstName = "Maria",
            LastName = "Wright",
            Affiliation = "Ottawa",
            FinishTimeRaw = "117.058", // 1:57.058 in raw seconds
            License = "L002"
        });

        race.Racers.Add(new Racer
        {
            Position = new Position("3"),
            RacerId = 116,
            LineNumber = 6,
            FirstName = "Mila",
            LastName = "Watson",
            Affiliation = "Barrie",
            FinishTimeRaw = "45.500", // 0:45.500 in raw seconds (under 1 minute)
            License = "L003"
        });

        var selectedRacers = race.Racers.Take(2).ToList();
        var newRaceNumber = "18C";
        var originalFilePath = Path.Combine(_tempDirectory, "18B-1-01.lif");

        // Act
        var (originalSplitPath, newSplitPath) = await _service.SplitRaceAsync(
            race, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Check that times are formatted as MM:SS.sss
        var lines = await File.ReadAllLinesAsync(newSplitPath);
        var racerLines = lines.Skip(1).ToArray(); // Skip race info line

        Assert.Equal(2, racerLines.Length);
        
        // Check first racer's finish time (should be 1:55.893)
        var firstRacerFields = racerLines[0].Split(',');
        var firstFinishTime = firstRacerFields[6]; // Finish time is at index 6
        Assert.Equal("1:55.893", firstFinishTime);

        // Check second racer's finish time (should be 1:57.058)
        var secondRacerFields = racerLines[1].Split(',');
        var secondFinishTime = secondRacerFields[6]; // Finish time is at index 6
        Assert.Equal("1:57.058", secondFinishTime);

        // Also check the original split file for the remaining racer
        var originalLines = await File.ReadAllLinesAsync(originalSplitPath);
        var originalRacerLines = originalLines.Skip(1).ToArray();

        Assert.Single(originalRacerLines);
        
        // Check remaining racer's finish time (should be 0:45.500)
        var remainingRacerFields = originalRacerLines[0].Split(',');
        var remainingFinishTime = remainingRacerFields[6];
        Assert.Equal("0:45.500", remainingFinishTime);
    }
}
