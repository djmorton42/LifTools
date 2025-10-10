using System;

namespace LifTools.Models;

public class FinishTime : IComparable<FinishTime>, IComparable
{
    public string RawSeconds { get; }
    
    public FinishTime(string rawSeconds)
    {
        RawSeconds = rawSeconds ?? string.Empty;
    }
    
    public override string ToString()
    {
        return RawSeconds;
    }
    
    public int CompareTo(FinishTime? other)
    {
        if (other == null) return 1;
        
        // Handle empty values - they should always sort last
        bool thisIsEmpty = string.IsNullOrEmpty(this.RawSeconds);
        bool otherIsEmpty = string.IsNullOrEmpty(other.RawSeconds);
        
        if (thisIsEmpty && otherIsEmpty) return 0;
        if (thisIsEmpty) return 1;  // This empty, other not empty -> this goes last
        if (otherIsEmpty) return -1; // This not empty, other empty -> other goes last
        
        // Try to parse both as numbers for numeric comparison
        if (double.TryParse(this.RawSeconds, out double thisSeconds) &&
            double.TryParse(other.RawSeconds, out double otherSeconds))
        {
            return thisSeconds.CompareTo(otherSeconds);
        }
        
        // If parsing fails, fall back to string comparison
        return string.Compare(this.RawSeconds, other.RawSeconds, StringComparison.Ordinal);
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is FinishTime other)
        {
            return CompareTo(other);
        }
        
        return 1;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is FinishTime other)
        {
            return RawSeconds == other.RawSeconds;
        }
        
        return false;
    }
    
    public override int GetHashCode()
    {
        return RawSeconds.GetHashCode();
    }
}
