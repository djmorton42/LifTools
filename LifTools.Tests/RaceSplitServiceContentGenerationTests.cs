using System;
using System.Collections.Generic;
using System.Linq;
using LifTools.Models;
using LifTools.Services;
using Xunit;

namespace LifTools.Tests;

public class RaceSplitServiceContentGenerationTests
{
    [Fact]
    public void GenerateSplitContent_ShouldGenerateCorrectContentWithoutFileIO()
    {
        // Arrange
        var service = new RaceSplitService();
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Take(2).ToList();
        var newRaceNumber = "21C";
        var originalFilePath = "21B-1-01.lif";

        // Act - No file I/O, just content generation
        var result = service.GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Verify file paths
        Assert.Contains("21B-1-01.lif", result.OriginalFilePath);
        Assert.Contains("21C-1-01.lif", result.NewFilePath);

        // Assert - Verify content is generated correctly
        Assert.NotEmpty(result.OriginalContent);
        Assert.NotEmpty(result.NewContent);

        // Assert - Verify content structure
        var originalLines = result.OriginalContent.Split(Environment.NewLine);
        var newLines = result.NewContent.Split(Environment.NewLine);

        // Should have race info + remaining racers
        Assert.Equal(3, originalLines.Length); // 1 race info + 2 remaining racers
        Assert.Equal(3, newLines.Length); // 1 race info + 2 selected racers

        // Verify race info lines
        Assert.Equal("21B,1,1,Test Event,,,,,,,10:00:00", originalLines[0]);
        Assert.Equal("21C,1,1,Test Event,,,,,,,10:00:00", newLines[0]);

        // Verify racer content
        Assert.Contains(originalLines, line => line.StartsWith("1,3,3,Third"));
        Assert.Contains(originalLines, line => line.StartsWith("2,4,4,Fourth"));
        Assert.Contains(newLines, line => line.StartsWith("1,1,1,First"));
        Assert.Contains(newLines, line => line.StartsWith("2,2,2,Second"));
    }

    [Fact]
    public void GenerateSplitContent_ShouldPreserveOriginalLaneNumbers()
    {
        // Arrange
        var service = new RaceSplitService();
        var originalRace = CreateTestRace();
        var selectedRacers = originalRace.Racers.Where(r => r.RacerId == 1).ToList(); // Select only first racer
        var newRaceNumber = "21C";
        var originalFilePath = "21B-1-01.lif";

        // Act
        var result = service.GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Original file should preserve lane numbers
        var originalLines = result.OriginalContent.Split(Environment.NewLine);
        Assert.Contains(originalLines, line => line.StartsWith("1,2,2,Second")); // Position 1, Lane 2 (original)
        Assert.Contains(originalLines, line => line.StartsWith("2,3,3,Third")); // Position 2, Lane 3 (original)
        Assert.Contains(originalLines, line => line.StartsWith("3,4,4,Fourth")); // Position 3, Lane 4 (original)

        // Assert - New file should renumber lanes
        var newLines = result.NewContent.Split(Environment.NewLine);
        Assert.Contains(newLines, line => line.StartsWith("1,1,1,First")); // Position 1, Lane 1 (renumbered)
    }

    [Fact]
    public void GenerateSplitContent_ShouldPreserveAlphaCodesInOriginalFile()
    {
        // Arrange
        var service = new RaceSplitService();
        var originalRace = CreateRaceWithAlphaCodes();
        var selectedRacers = originalRace.Racers.Where(r => r.RacerId == 1 || r.RacerId == 2).ToList(); // Select numeric racers
        var newRaceNumber = "21C";
        var originalFilePath = "21B-1-01.lif";

        // Act
        var result = service.GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Alpha codes should remain unchanged in original file
        var originalLines = result.OriginalContent.Split(Environment.NewLine);
        Assert.Contains(originalLines, line => line.StartsWith("1,3,3,Third")); // Position 1, Lane 3 (original)
        Assert.Contains(originalLines, line => line.StartsWith("2,4,4,Fourth")); // Position 2, Lane 4 (original)
        Assert.Contains(originalLines, line => line.StartsWith("DNS,5,5,DNS")); // DNS should remain DNS
        Assert.Contains(originalLines, line => line.StartsWith("DNF,6,6,DNF")); // DNF should remain DNF
    }

    [Fact]
    public void GenerateSplitContent_ShouldPreserveAlphaCodesInNewFile()
    {
        // Arrange
        var service = new RaceSplitService();
        var originalRace = CreateRaceWithAlphaCodes();
        var selectedRacers = originalRace.Racers.Where(r => r.RacerId == 5 || r.RacerId == 6).ToList(); // Select alpha code racers
        var newRaceNumber = "21C";
        var originalFilePath = "21B-1-01.lif";

        // Act
        var result = service.GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Alpha codes should remain unchanged in new file
        var newLines = result.NewContent.Split(Environment.NewLine);
        Assert.Contains(newLines, line => line.StartsWith("DNS,5,2,DNS")); // DNS should remain DNS, lane renumbered
        Assert.Contains(newLines, line => line.StartsWith("DNF,6,1,DNF")); // DNF should remain DNF, lane renumbered
    }

    [Fact]
    public void GenerateSplitContent_ShouldPreserveAlphaCodesWhenMixedWithNumericRacers()
    {
        // Arrange
        var service = new RaceSplitService();
        var originalRace = CreateRaceWithAlphaCodes();
        var selectedRacers = originalRace.Racers.Where(r => r.RacerId == 1 || r.RacerId == 5).ToList(); // Select one numeric, one alpha
        var newRaceNumber = "21C";
        var originalFilePath = "21B-1-01.lif";

        // Act
        var result = service.GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);

        // Assert - Mixed selection should preserve alpha codes
        var newLines = result.NewContent.Split(Environment.NewLine);
        Assert.Contains(newLines, line => line.StartsWith("1,1,1,First")); // Numeric position renumbered
        Assert.Contains(newLines, line => line.StartsWith("DNS,5,2,DNS")); // Alpha code preserved, lane renumbered

        var originalLines = result.OriginalContent.Split(Environment.NewLine);
        Assert.Contains(originalLines, line => line.StartsWith("1,2,2,Second")); // Numeric position renumbered
        Assert.Contains(originalLines, line => line.StartsWith("2,3,3,Third")); // Numeric position renumbered
        Assert.Contains(originalLines, line => line.StartsWith("3,4,4,Fourth")); // Numeric position renumbered
        Assert.Contains(originalLines, line => line.StartsWith("DNF,6,6,DNF")); // Alpha code preserved
    }

    private Race CreateRaceWithAlphaCodes()
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

        // Create racers with numeric positions
        for (int i = 1; i <= 4; i++)
        {
            race.Racers.Add(new Racer
            {
                Position = new Position(i.ToString()),
                RacerId = i,
                LineNumber = i,
                FirstName = $"Name{i}",
                LastName = GetOrdinalName(i),
                Affiliation = $"Club{i}",
                FinishTimeRaw = (50.0 + i).ToString("F3"),
                License = $"L{i:D3}"
            });
        }

        // Add racers with alpha codes
        race.Racers.Add(new Racer
        {
            Position = new Position("DNS"),
            RacerId = 5,
            LineNumber = 5,
            FirstName = "DNS",
            LastName = "DNS",
            Affiliation = "Club5",
            FinishTimeRaw = "0.000",
            License = "L005"
        });

        race.Racers.Add(new Racer
        {
            Position = new Position("DNF"),
            RacerId = 6,
            LineNumber = 6,
            FirstName = "DNF",
            LastName = "DNF",
            Affiliation = "Club6",
            FinishTimeRaw = "0.000",
            License = "L006"
        });

        return race;
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

        // Create 4 test racers
        for (int i = 1; i <= 4; i++)
        {
            race.Racers.Add(new Racer
            {
                Position = new Position(i.ToString()),
                RacerId = i,
                LineNumber = i,
                FirstName = $"Name{i}",
                LastName = GetOrdinalName(i),
                Affiliation = $"Club{i}",
                FinishTimeRaw = (50.0 + i).ToString("F3"),
                License = $"L{i:D3}"
            });
        }

        return race;
    }

    private string GetOrdinalName(int number)
    {
        return number switch
        {
            1 => "First",
            2 => "Second", 
            3 => "Third",
            4 => "Fourth",
            _ => $"Racer{number}"
        };
    }
}
