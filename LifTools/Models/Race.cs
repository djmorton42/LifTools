using System.Collections.Generic;

namespace LifTools.Models;

public class Race
{
    public RaceInfo RaceInfo { get; set; } = new();
    public List<Racer> Racers { get; set; } = new();

    public override string ToString()
    {
        var result = $"{RaceInfo}\n";
        result += $"Racers ({Racers.Count}):\n";
        foreach (var racer in Racers)
        {
            result += $"  {racer}\n";
        }
        return result.TrimEnd();
    }
}
