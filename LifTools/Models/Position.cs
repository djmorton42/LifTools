using System;
using System.ComponentModel;

namespace LifTools.Models;

public class Position : IComparable<Position>, IComparable
{
    public string Value { get; }
    public bool IsNumeric { get; }
    public int NumericValue { get; }

    public Position(string value)
    {
        Value = value ?? string.Empty;
        IsNumeric = int.TryParse(Value, out int numericValue);
        NumericValue = numericValue;
    }

    public int CompareTo(Position? other)
    {
        if (other == null) return 1;

        // If both are numeric, compare as integers
        if (IsNumeric && other.IsNumeric)
        {
            return NumericValue.CompareTo(other.NumericValue);
        }

        // If only this is numeric, this comes first
        if (IsNumeric && !other.IsNumeric)
        {
            return -1;
        }

        // If only other is numeric, other comes first
        if (!IsNumeric && other.IsNumeric)
        {
            return 1;
        }

        // If neither is numeric, compare alphabetically
        return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public int CompareTo(object? obj)
    {
        if (obj is Position other)
        {
            return CompareTo(other);
        }
        return 1;
    }

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Position other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(Position position)
    {
        return position.Value;
    }

    public static implicit operator Position(string value)
    {
        return new Position(value);
    }
}
