using System;

namespace LifTools.Models;

public class Racer : IComparable<Racer>
{
    public Position Position { get; set; } = new Position(string.Empty);
    public int RacerId { get; set; }
    public int LineNumber { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Affiliation { get; set; } = string.Empty;
    public string FinishTime { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string DeltaTime { get; set; } = string.Empty;
    public string ReacTime { get; set; } = string.Empty;
    public string Splits { get; set; } = string.Empty;
    public string RaceStartTime { get; set; } = string.Empty;
    public string Unused1 { get; set; } = string.Empty;
    public string Unused2 { get; set; } = string.Empty;
    public string Unused3 { get; set; } = string.Empty;
    public string Unused4 { get; set; } = string.Empty;
    public string Unused5 { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"#{Position}: {FirstName} {LastName} ({Affiliation}) - {FinishTime} (Lane {LineNumber}, ID: {RacerId})";
    }

    public int CompareTo(Racer? other)
    {
        if (other == null) return 1;
        
        // Use the Position's built-in comparison
        return this.Position.CompareTo(other.Position);
    }
}
