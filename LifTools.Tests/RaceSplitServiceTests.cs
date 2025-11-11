using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LifTools.Models;
using LifTools.Services;
using Xunit;

namespace LifTools.Tests;

public class RaceSplitServiceTests : IDisposable
{
    private readonly RaceSplitService _service;
    private readonly string _tempDirectory;

    public RaceSplitServiceTests()
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
    public async Task SplitRaceAsync_ShouldCreateTwoFiles()
    {
        // Arrange
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = Path.Combine(_tempDirectory, "21B-1-01.lif");
        
        // Create the original file first
        await File.WriteAllTextAsync(originalFilePath, "test content");

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert
        Assert.True(File.Exists(originalSplitPath));
        Assert.True(File.Exists(newSplitPath));
        Assert.Contains("21B-1-01.lif", originalSplitPath);
        Assert.Contains("21C-1-01.lif", newSplitPath);
    }

    [Fact]
    public async Task SplitRaceAsync_ShouldCreateBackupFile()
    {
        // Arrange
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = Path.Combine(_tempDirectory, "21B-1-01.lif");
        var backupFilePath = Path.Combine(_tempDirectory, "21B-1-01-original.lif");
        
        // Create the original file first with some content
        var originalContent = "original file content";
        await File.WriteAllTextAsync(originalFilePath, originalContent);

        // Act
        await _service.SplitRaceAsync(
            originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Verify backup file exists and contains original content
        Assert.True(File.Exists(backupFilePath));
        var backupContent = await File.ReadAllTextAsync(backupFilePath);
        Assert.Equal(originalContent, backupContent);
    }

    [Fact]
    public async Task SplitRaceAsync_ShouldUpdatePositionsAndLanes()
    {
        // Arrange
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = Path.Combine(_tempDirectory, "21B-1-01.lif");
        
        // Create the original file first
        await File.WriteAllTextAsync(originalFilePath, "test content");

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Check that the new race file has correct positions and lanes
        var lines = await File.ReadAllLinesAsync(newSplitPath);
        var racerLines = lines.Skip(1).ToArray(); // Skip race info line

        Assert.Equal(2, racerLines.Length);
        
        // Check first racer
        var firstRacerFields = racerLines[0].Split(',');
        Assert.Equal("1", firstRacerFields[0]); // Position should be 1
        Assert.Equal("1", firstRacerFields[2]); // Lane should be 1

        // Check second racer
        var secondRacerFields = racerLines[1].Split(',');
        Assert.Equal("2", secondRacerFields[0]); // Position should be 2
        Assert.Equal("2", secondRacerFields[2]); // Lane should be 2
    }

    [Fact]
    public async Task SplitRaceAsync_ShouldUpdateRaceNumbers()
    {
        // Arrange
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = Path.Combine(_tempDirectory, "21B-1-01.lif");
        
        // Create the original file first
        await File.WriteAllTextAsync(originalFilePath, "test content");

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Check race numbers in the files
        var originalLines = await File.ReadAllLinesAsync(originalSplitPath);
        var newLines = await File.ReadAllLinesAsync(newSplitPath);

        var originalRaceInfo = originalLines[0].Split(',');
        var newRaceInfo = newLines[0].Split(',');

        Assert.Equal("21B", originalRaceInfo[0]); // Original race number
        Assert.Equal("21C", newRaceInfo[0]); // New race number
    }

    [Fact]
    public async Task SplitRaceAsync_ShouldFormatTimesAsMinutesSeconds()
    {
        // Arrange
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = Path.Combine(_tempDirectory, "21B-1-01.lif");
        
        // Create the original file first
        await File.WriteAllTextAsync(originalFilePath, "test content");

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Check that times are formatted as MM:SS.sss
        var lines = await File.ReadAllLinesAsync(newSplitPath);
        var racerLines = lines.Skip(1).ToArray(); // Skip race info line

        Assert.Equal(2, racerLines.Length);
        
        // Check first racer's finish time format
        var firstRacerFields = racerLines[0].Split(',');
        var firstFinishTime = firstRacerFields[6]; // Finish time is at index 6
        Assert.Matches(@"^\d+:\d{2}\.\d{3}$", firstFinishTime); // Should match MM:SS.sss format

        // Check second racer's finish time format
        var secondRacerFields = racerLines[1].Split(',');
        var secondFinishTime = secondRacerFields[6]; // Finish time is at index 6
        Assert.Matches(@"^\d+:\d{2}\.\d{3}$", secondFinishTime); // Should match MM:SS.sss format
    }

    private Race CreateTestRace()
    {
        var race = new Race
        {
            RaceInfo = new RaceInfo
            {
                RaceNumber = "21B",
                Heat = 1,
                Round = 1,
                EventName = "Test Event",
                StartTime = "10:00:00"
            }
        };

        // Create 5 test racers with different positions and lanes
        for (int i = 1; i <= 5; i++)
        {
            race.Racers.Add(new Racer
            {
                Position = new Position(i.ToString()),
                RacerId = i,
                LineNumber = i,
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                Affiliation = $"Club{i}",
                FinishTimeRaw = (50.0 + i).ToString("F3"),
                License = $"L{i:D3}"
            });
        }

        return race;
    }
}
