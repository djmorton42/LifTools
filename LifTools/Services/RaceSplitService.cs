using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifTools.Models;

namespace LifTools.Services;

public class SplitResult
{
    public string OriginalFilePath { get; set; } = string.Empty;
    public string NewFilePath { get; set; } = string.Empty;
    public string OriginalContent { get; set; } = string.Empty;
    public string NewContent { get; set; } = string.Empty;
}

public class RaceSplitService
{
    public SplitResult GenerateSplitContent(
        Race originalRace, 
        List<Racer> selectedRacers, 
        string newRaceNumber, 
        string originalFilePath)
    {
        if (originalRace == null) throw new ArgumentNullException(nameof(originalRace));
        if (selectedRacers == null) throw new ArgumentNullException(nameof(selectedRacers));
        if (string.IsNullOrEmpty(originalFilePath)) throw new ArgumentException("Original file path cannot be null or empty", nameof(originalFilePath));
        if (string.IsNullOrEmpty(newRaceNumber)) throw new ArgumentException("New race number cannot be null or empty", nameof(newRaceNumber));

        var directory = Path.GetDirectoryName(originalFilePath) ?? string.Empty;
        var originalFileName = Path.GetFileName(originalFilePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);

        // Extract race number from filename (e.g., "21B-1-01" -> "21B")
        var parts = fileNameWithoutExtension.Split('-');
        if (parts.Length < 1)
        {
            throw new ArgumentException("Invalid file name format", nameof(originalFilePath));
        }

        var currentRaceNumber = parts[0];

        // Create file names
        var originalSplitFileName = $"{currentRaceNumber}-1-01-split{extension}";
        var newSplitFileName = $"{newRaceNumber}-1-01-split{extension}";

        var originalSplitFilePath = Path.Combine(directory, originalSplitFileName);
        var newSplitFilePath = Path.Combine(directory, newSplitFileName);

        // Create the split races
        var originalSplitRace = CreateSplitRace(originalRace, selectedRacers, false);
        var newSplitRace = CreateSplitRace(originalRace, selectedRacers, true);

        // Update race numbers
        originalSplitRace.RaceInfo.RaceNumber = currentRaceNumber;
        newSplitRace.RaceInfo.RaceNumber = newRaceNumber;

        // Generate content
        var originalContent = GenerateRaceContent(originalSplitRace);
        var newContent = GenerateRaceContent(newSplitRace);

        return new SplitResult
        {
            OriginalFilePath = originalSplitFilePath,
            NewFilePath = newSplitFilePath,
            OriginalContent = originalContent,
            NewContent = newContent
        };
    }

    public async Task<(string originalFilePath, string newFilePath)> SplitRaceAsync(
        Race originalRace, 
        List<Racer> selectedRacers, 
        string newRaceNumber, 
        string originalFilePath)
    {
        var result = GenerateSplitContent(originalRace, selectedRacers, newRaceNumber, originalFilePath);
        
        // Write the files
        await File.WriteAllTextAsync(result.OriginalFilePath, result.OriginalContent, Encoding.UTF8);
        await File.WriteAllTextAsync(result.NewFilePath, result.NewContent, Encoding.UTF8);

        return (result.OriginalFilePath, result.NewFilePath);
    }

    private Race CreateSplitRace(Race originalRace, List<Racer> selectedRacers, bool includeSelected)
    {
        var splitRace = new Race
        {
            RaceInfo = new RaceInfo
            {
                RaceNumber = originalRace.RaceInfo.RaceNumber,
                Heat = originalRace.RaceInfo.Heat,
                Round = originalRace.RaceInfo.Round,
                EventName = originalRace.RaceInfo.EventName,
                Unused1 = originalRace.RaceInfo.Unused1,
                Unused2 = originalRace.RaceInfo.Unused2,
                Unused3 = originalRace.RaceInfo.Unused3,
                Unused4 = originalRace.RaceInfo.Unused4,
                Unused5 = originalRace.RaceInfo.Unused5,
                Unused6 = originalRace.RaceInfo.Unused6,
                StartTime = originalRace.RaceInfo.StartTime
            }
        };

        // Filter racers based on whether we want selected or unselected
        var racersToInclude = includeSelected 
            ? selectedRacers.ToList() 
            : originalRace.Racers.Except(selectedRacers).ToList();

        // Sort racers: numeric positions first (sorted numerically), then alpha codes (sorted alphabetically)
        racersToInclude.Sort((a, b) =>
        {
            var aIsNumeric = a.Position.IsNumeric;
            var bIsNumeric = b.Position.IsNumeric;
            
            // If both are numeric, sort numerically
            if (aIsNumeric && bIsNumeric)
            {
                return int.Parse(a.Position.Value).CompareTo(int.Parse(b.Position.Value));
            }
            
            // If both are alpha codes, sort alphabetically
            if (!aIsNumeric && !bIsNumeric)
            {
                return string.Compare(a.Position.Value, b.Position.Value, StringComparison.OrdinalIgnoreCase);
            }
            
            // Numeric positions come before alpha codes
            return aIsNumeric ? -1 : 1;
        });

        // Update positions and lane numbers
        UpdatePositionsAndLanes(racersToInclude, includeSelected);

        splitRace.Racers.AddRange(racersToInclude);

        return splitRace;
    }

    private void UpdatePositionsAndLanes(List<Racer> racers, bool includeSelected)
    {
        for (int i = 0; i < racers.Count; i++)
        {
            var racer = racers[i];
            
            // Check if position is an alpha code (like DNS, DNF, etc.) - these should never change
            if (!racer.Position.IsNumeric)
            {
                // Alpha codes should never be changed, regardless of split operation
                // Only update lane numbers for the new split file
                if (includeSelected)
                {
                    racer.LineNumber = i + 1;
                }
                continue;
            }
            
            // Update position to be sequential starting from 1 (only for numeric positions)
            racer.Position = new Position((i + 1).ToString());
            
            // Only update lane numbers for the new split file (includeSelected = true)
            // For the original file (includeSelected = false), keep original lane numbers
            if (includeSelected)
            {
                racer.LineNumber = i + 1;
            }
        }
    }

    private string GenerateRaceContent(Race race)
    {
        var lines = new List<string>();

        // Add race info line
        var raceInfoLine = CreateRaceInfoLine(race.RaceInfo);
        lines.Add(raceInfoLine);

        // Add racer lines
        foreach (var racer in race.Racers)
        {
            var racerLine = CreateRacerLine(racer);
            lines.Add(racerLine);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string CreateRaceInfoLine(RaceInfo raceInfo)
    {
        var fields = new[]
        {
            raceInfo.RaceNumber,
            raceInfo.Heat.ToString(),
            raceInfo.Round.ToString(),
            raceInfo.EventName,
            raceInfo.Unused1,
            raceInfo.Unused2,
            raceInfo.Unused3,
            raceInfo.Unused4,
            raceInfo.Unused5,
            raceInfo.Unused6,
            raceInfo.StartTime
        };

        return string.Join(",", fields.Select(EscapeCsvField));
    }

    private string CreateRacerLine(Racer racer)
    {
        var fields = new[]
        {
            racer.Position.Value,
            racer.RacerId.ToString(),
            racer.LineNumber.ToString(),
            racer.LastName,
            racer.FirstName,
            racer.Affiliation,
            FormatTimeForSplit(racer.FinishTimeRaw),
            racer.License,
            racer.DeltaTime,
            racer.ReacTime,
            racer.Splits,
            racer.RaceStartTime,
            racer.Unused1,
            racer.Unused2,
            racer.Unused3,
            racer.Unused4,
            racer.Unused5
        };

        return string.Join(",", fields.Select(EscapeCsvField));
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private string FormatTimeForSplit(string rawSecondsString)
    {
        if (string.IsNullOrEmpty(rawSecondsString))
        {
            return rawSecondsString;
        }
        
        // Try to parse the raw seconds
        if (double.TryParse(rawSecondsString, out double totalSeconds))
        {
            int minutes = (int)(totalSeconds / 60);
            double seconds = totalSeconds % 60;
            
            // Always format as MM:SS.sss, even for times under 1 minute
            return $"{minutes}:{seconds:00.000}";
        }
        
        return rawSecondsString; // Return original if parsing fails
    }
}
