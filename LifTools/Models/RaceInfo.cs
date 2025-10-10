namespace LifTools.Models;

public class RaceInfo
{
    public string RaceNumber { get; set; } = string.Empty;
    public int Heat { get; set; }
    public int Round { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Unused1 { get; set; } = string.Empty;
    public string Unused2 { get; set; } = string.Empty;
    public string Unused3 { get; set; } = string.Empty;
    public string Unused4 { get; set; } = string.Empty;
    public string Unused5 { get; set; } = string.Empty;
    public string Unused6 { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Race: {RaceNumber} - Heat {Heat}, Round {Round} - {EventName} (Start: {StartTime})";
    }
}
