using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LifTools.Models;
using LifTools.Services;
using Xunit;

namespace LifTools.Tests;

public class RealLifFileSplitTests : IDisposable
{
    private readonly RaceSplitService _service;
    private readonly string _tempDirectory;
    private readonly string _testLifFilePath;

    public RealLifFileSplitTests()
    {
        _service = new RaceSplitService();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        
        // Copy the test LIF file to temp directory
        var originalLifPath = Path.Combine("..", "..", "..", "..", "etc", "18B-1-01.lif");
        _testLifFilePath = Path.Combine(_tempDirectory, "18B-1-01.lif");
        File.Copy(originalLifPath, _testLifFilePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task SplitRaceAsync_WithRealLifFile_ShouldProduceCorrectOutput()
    {
        // Arrange - Load the real LIF file
        var dataProvider = new FileDataProvider(_testLifFilePath);
        var parser = new LifParserService(dataProvider);
        var originalRace = await parser.ParseRaceAsync();

        // Find the racers to split (racer 392 in lane 5 and racer 116 in lane 6)
        var selectedRacers = originalRace.Racers
            .Where(r => r.RacerId == 392 || r.RacerId == 116)
            .ToList();

        Assert.Equal(2, selectedRacers.Count);
        Assert.Contains(selectedRacers, r => r.RacerId == 392 && r.LineNumber == 5);
        Assert.Contains(selectedRacers, r => r.RacerId == 116 && r.LineNumber == 6);

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, "18C", _testLifFilePath);

        // Assert - Verify file names
        Assert.Contains("18B-1-01.lif", originalSplitPath);
        Assert.Contains("18C-1-01.lif", newSplitPath);

        // Assert - Verify original split file content
        var originalLines = await File.ReadAllLinesAsync(originalSplitPath);
        
        // Check race info line
        Assert.Equal("18B,1,1,Open Women B (1000 111M) Final,,,,,,,14:25:33.4364", originalLines[0]);
        
        // Check we have 4 racers
        Assert.Equal(5, originalLines.Length); // 1 race info + 4 racers
        
        // Check specific racers are present with correct positions and lanes
        Assert.Contains(originalLines, line => line.StartsWith("1,177,2,Wright,Maria,Ottawa,1:57.058"));
        Assert.Contains(originalLines, line => line.StartsWith("2,303,4,Stewart,Brooklyn,Gloucester,1:58.433"));
        Assert.Contains(originalLines, line => line.StartsWith("3,274,1,Cox,Paisley,Ottawa,1:59.090"));
        Assert.Contains(originalLines, line => line.StartsWith("4,440,3,Flores,Evelyn,Kingston,1:59.225"));
        
        // Verify selected racers are NOT in the original file
        Assert.DoesNotContain(originalLines, line => line.Contains("392,King,Sofia"));
        Assert.DoesNotContain(originalLines, line => line.Contains("116,Watson,Mila"));

        // Assert - Verify new split file content
        var newLines = await File.ReadAllLinesAsync(newSplitPath);
        
        // Check race info line
        Assert.Equal("18C,1,1,Open Women B (1000 111M) Final,,,,,,,14:25:33.4364", newLines[0]);
        
        // Check we have 2 racers
        Assert.Equal(3, newLines.Length); // 1 race info + 2 racers
        
        // Check specific racers are present with correct positions and lanes
        Assert.Contains(newLines, line => line.StartsWith("1,392,1,King,Sofia,Ottawa,1:55.893"));
        Assert.Contains(newLines, line => line.StartsWith("2,116,2,Watson,Mila,Barrie,1:57.451"));
        
        // Verify non-selected racers are NOT in the new file
        Assert.DoesNotContain(newLines, line => line.Contains("177,Wright,Maria"));
        Assert.DoesNotContain(newLines, line => line.Contains("303,Stewart,Brooklyn"));
        Assert.DoesNotContain(newLines, line => line.Contains("274,Cox,Paisley"));
        Assert.DoesNotContain(newLines, line => line.Contains("440,Flores,Evelyn"));
    }

    [Fact]
    public async Task SplitRaceAsync_WithRealLifFile_ShouldUpdatePositionsAndLanesCorrectly()
    {
        // Arrange
        var dataProvider = new FileDataProvider(_testLifFilePath);
        var parser = new LifParserService(dataProvider);
        var originalRace = await parser.ParseRaceAsync();

        var selectedRacers = originalRace.Racers
            .Where(r => r.RacerId == 392 || r.RacerId == 116)
            .ToList();

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, "18C", _testLifFilePath);

        // Assert - Check original split file positions and lanes
        var originalLines = await File.ReadAllLinesAsync(originalSplitPath);
        var originalRacerLines = originalLines.Skip(1).ToArray();

        Assert.Equal(4, originalRacerLines.Length); // Should have 4 remaining racers

        // Check positions are sequential starting from 1, but lanes should be original
        for (int i = 0; i < originalRacerLines.Length; i++)
        {
            var fields = originalRacerLines[i].Split(',');
            Assert.Equal((i + 1).ToString(), fields[0]); // Position should be i+1
            // Lane numbers should remain original (not sequential)
        }
        
        // Verify specific lane numbers are preserved from original
        Assert.Contains(originalRacerLines, line => line.StartsWith("1,177,2,")); // Maria Wright should keep lane 2
        Assert.Contains(originalRacerLines, line => line.StartsWith("2,303,4,")); // Brooklyn Stewart should keep lane 4
        Assert.Contains(originalRacerLines, line => line.StartsWith("3,274,1,")); // Paisley Cox should keep lane 1
        Assert.Contains(originalRacerLines, line => line.StartsWith("4,440,3,")); // Evelyn Flores should keep lane 3

        // Assert - Check new split file positions and lanes
        var newLines = await File.ReadAllLinesAsync(newSplitPath);
        var newRacerLines = newLines.Skip(1).ToArray();

        Assert.Equal(2, newRacerLines.Length); // Should have 2 selected racers

        // Check positions are sequential starting from 1
        for (int i = 0; i < newRacerLines.Length; i++)
        {
            var fields = newRacerLines[i].Split(',');
            Assert.Equal((i + 1).ToString(), fields[0]); // Position should be i+1
            Assert.Equal((i + 1).ToString(), fields[2]); // Lane should be i+1
        }

        // Verify specific racers are in the new file
        var newFileContent = string.Join("\n", newLines);
        Assert.Contains("392,1,King,Sofia", newFileContent); // Racer 392 should be lane 1
        Assert.Contains("116,2,Watson,Mila", newFileContent); // Racer 116 should be lane 2
    }

    [Fact]
    public async Task SplitRaceAsync_WithRealLifFile_ShouldMaintainTimeFormatting()
    {
        // Arrange
        var dataProvider = new FileDataProvider(_testLifFilePath);
        var parser = new LifParserService(dataProvider);
        var originalRace = await parser.ParseRaceAsync();

        var selectedRacers = originalRace.Racers
            .Where(r => r.RacerId == 392 || r.RacerId == 116)
            .ToList();

        // Act
        var (originalSplitPath, newSplitPath, backupPath) = await _service.SplitRaceAsync(
            originalRace, selectedRacers, "18C", _testLifFilePath);

        // Assert - Check that times are formatted as MM:SS.sss
        var originalLines = await File.ReadAllLinesAsync(originalSplitPath);
        var newLines = await File.ReadAllLinesAsync(newSplitPath);

        // Check original file times
        foreach (var line in originalLines.Skip(1))
        {
            var fields = line.Split(',');
            var finishTime = fields[6]; // Finish time field
            Assert.Matches(@"^\d+:\d{2}\.\d{3}$", finishTime);
        }

        // Check new file times
        foreach (var line in newLines.Skip(1))
        {
            var fields = line.Split(',');
            var finishTime = fields[6]; // Finish time field
            Assert.Matches(@"^\d+:\d{2}\.\d{3}$", finishTime);
        }

        // Verify specific times are preserved correctly
        var newFileContent = string.Join("\n", newLines);
        Assert.Contains("1:55.893", newFileContent); // Sofia King's time
        Assert.Contains("1:57.451", newFileContent); // Mila Watson's time
    }

    [Fact]
    public async Task SplitRaceAsync_WithRealLifFile_ShouldCreateBackupFile()
    {
        // Arrange - Load the real LIF file
        var dataProvider = new FileDataProvider(_testLifFilePath);
        var parser = new LifParserService(dataProvider);
        var originalRace = await parser.ParseRaceAsync();

        var selectedRacers = originalRace.Racers
            .Where(r => r.RacerId == 392 || r.RacerId == 116)
            .ToList();

        // Read original file content before split
        var originalContent = await File.ReadAllTextAsync(_testLifFilePath);
        var backupFilePath = Path.Combine(Path.GetDirectoryName(_testLifFilePath) ?? string.Empty, "18B-1-01-original.lif");

        // Act
        await _service.SplitRaceAsync(
            originalRace, selectedRacers, "18C", _testLifFilePath);

        // Assert - Verify backup file exists and contains original content
        Assert.True(File.Exists(backupFilePath));
        var backupContent = await File.ReadAllTextAsync(backupFilePath);
        Assert.Equal(originalContent, backupContent);
    }
}
