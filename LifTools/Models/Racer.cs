using System;
using System.ComponentModel;

namespace LifTools.Models;

public class Racer : IComparable<Racer>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Position Position { get; set; } = new Position(string.Empty);
    public int RacerId { get; set; }
    public int LineNumber { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Affiliation { get; set; } = string.Empty;
    public string FinishTimeRaw { get; set; } = string.Empty;        // e.g., "69.622"
    public string FinishTimeFormatted { get; set; } = string.Empty;  // e.g., "1:09.622"
    private string _displayFinishTime = string.Empty;
    public string DisplayFinishTime 
    { 
        get => _displayFinishTime;
        set
        {
            if (_displayFinishTime != value)
            {
                _displayFinishTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayFinishTime)));
            }
        }
    }
    
    // Sortable finish time property that compares by raw seconds
    public FinishTime FinishTime { get; set; } = new FinishTime(string.Empty);
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
        return $"#{Position}: {FirstName} {LastName} ({Affiliation}) - {FinishTimeRaw} (Lane {LineNumber}, ID: {RacerId})";
    }

    public int CompareTo(Racer? other)
    {
        if (other == null) return 1;
        
        // Use the Position's built-in comparison
        return this.Position.CompareTo(other.Position);
    }
}
