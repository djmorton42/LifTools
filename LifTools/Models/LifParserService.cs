using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LifTools.Models;

public class LifParserService
{
    private readonly IDataProvider _dataProvider;

    public LifParserService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    public async Task<Race> ParseRaceAsync()
    {
        var lines = await _dataProvider.GetLinesAsync();
        var linesList = lines.ToList();

        if (linesList.Count == 0)
        {
            throw new InvalidOperationException("LIF file is empty");
        }

        var race = new Race();

        // Parse race info from first line
        race.RaceInfo = ParseRaceInfo(linesList[0]);

        // Parse racers from remaining lines
        for (int i = 1; i < linesList.Count; i++)
        {
            var racer = ParseRacer(linesList[i]);
            race.Racers.Add(racer);
        }

        return race;
    }

    private static RaceInfo ParseRaceInfo(string line)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            MissingFieldFound = null
        };

        using var reader = new StringReader(line);
        using var csv = new CsvReader(reader, config);
        
        if (!csv.Read())
        {
            throw new InvalidOperationException("Failed to read race info line");
        }
        
        var record = csv.GetRecord<RaceInfoRecord>();
        if (record == null)
        {
            throw new InvalidOperationException("Failed to parse race info line");
        }

        return new RaceInfo
        {
            RaceNumber = record.RaceNumber ?? string.Empty,
            Heat = int.TryParse(record.Heat, out var heat) ? heat : 0,
            Round = int.TryParse(record.Round, out var round) ? round : 0,
            EventName = record.EventName ?? string.Empty,
            Unused1 = record.Unused1 ?? string.Empty,
            Unused2 = record.Unused2 ?? string.Empty,
            Unused3 = record.Unused3 ?? string.Empty,
            Unused4 = record.Unused4 ?? string.Empty,
            Unused5 = record.Unused5 ?? string.Empty,
            Unused6 = record.Unused6 ?? string.Empty,
            StartTime = record.StartTime ?? string.Empty
        };
    }

    private static Racer ParseRacer(string line)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            MissingFieldFound = null
        };

        using var reader = new StringReader(line);
        using var csv = new CsvReader(reader, config);
        
        if (!csv.Read())
        {
            throw new InvalidOperationException("Failed to read racer line");
        }
        
        var record = csv.GetRecord<RacerRecord>();
        if (record == null)
        {
            throw new InvalidOperationException("Failed to parse racer line");
        }

        var finishTime = record.FinishTime ?? string.Empty;
        var rawTime = ParseTimeToSeconds(finishTime);
        var formattedTime = FormatTime(rawTime);
        return new Racer
        {
            Position = new Position(record.Position ?? string.Empty),
            RacerId = int.TryParse(record.RacerId, out var racerId) ? racerId : 0,
            LineNumber = int.TryParse(record.LineNumber, out var lineNumber) ? lineNumber : 0,
            LastName = record.LastName ?? string.Empty,
            FirstName = record.FirstName ?? string.Empty,
            Affiliation = record.Affiliation ?? string.Empty,
            FinishTimeRaw = rawTime,
            FinishTimeFormatted = formattedTime,
            FinishTime = new FinishTime(rawTime),
            License = record.License ?? string.Empty,
            DeltaTime = record.DeltaTime ?? string.Empty,
            ReacTime = record.ReacTime ?? string.Empty,
            Splits = record.Splits ?? string.Empty,
            RaceStartTime = record.RaceStartTime ?? string.Empty,
            Unused1 = record.Unused1 ?? string.Empty,
            Unused2 = record.Unused2 ?? string.Empty,
            Unused3 = record.Unused3 ?? string.Empty,
            Unused4 = record.Unused4 ?? string.Empty,
            Unused5 = record.Unused5 ?? string.Empty
        };
    }

    private static string ParseTimeToSeconds(string timeString)
    {
        if (string.IsNullOrEmpty(timeString))
        {
            return string.Empty;
        }
        
        // Try to parse as already raw seconds (e.g., "46.529")
        if (double.TryParse(timeString, out double rawSeconds))
        {
            return rawSeconds.ToString("F3");
        }
        
        // Try to parse as formatted time (e.g., "1:55.893")
        if (timeString.Contains(':'))
        {
            var parts = timeString.Split(':');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int minutes) && 
                double.TryParse(parts[1], out double seconds))
            {
                var totalSeconds = minutes * 60 + seconds;
                return totalSeconds.ToString("F3");
            }
        }
        
        // Return original if parsing fails
        return timeString;
    }

    private static string FormatTime(string rawSecondsString)
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
            
            // Only show minutes if it's 1 or more
            if (minutes > 0)
            {
                return $"{minutes}:{seconds:00.000}";
            }
            else
            {
                return $"{seconds:F3}";
            }
        }
        
        return rawSecondsString; // Return original if parsing fails
    }

    // Helper classes for CSV parsing
    private class RaceInfoRecord
    {
        [Index(0)]
        public string? RaceNumber { get; set; }
        [Index(1)]
        public string? Heat { get; set; }
        [Index(2)]
        public string? Round { get; set; }
        [Index(3)]
        public string? EventName { get; set; }
        [Index(4)]
        public string? Unused1 { get; set; }
        [Index(5)]
        public string? Unused2 { get; set; }
        [Index(6)]
        public string? Unused3 { get; set; }
        [Index(7)]
        public string? Unused4 { get; set; }
        [Index(8)]
        public string? Unused5 { get; set; }
        [Index(9)]
        public string? Unused6 { get; set; }
        [Index(10)]
        public string? StartTime { get; set; }
    }

    private class RacerRecord
    {
        public string? Position { get; set; }
        public string? RacerId { get; set; }
        public string? LineNumber { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Affiliation { get; set; }
        public string? FinishTime { get; set; }
        public string? License { get; set; }
        public string? DeltaTime { get; set; }
        public string? ReacTime { get; set; }
        public string? Splits { get; set; }
        public string? RaceStartTime { get; set; }
        public string? Unused1 { get; set; }
        public string? Unused2 { get; set; }
        public string? Unused3 { get; set; }
        public string? Unused4 { get; set; }
        public string? Unused5 { get; set; }
    }
}
