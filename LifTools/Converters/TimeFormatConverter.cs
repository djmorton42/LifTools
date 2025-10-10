using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using LifTools.Models;

namespace LifTools.Converters;

public class TimeFormatConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 3 && 
            values[0] is TimeFormatMode timeFormatMode &&
            values[1] is string rawTime &&
            values[2] is string formattedTime)
        {
            System.Diagnostics.Debug.WriteLine($"Converter: timeFormatMode={timeFormatMode}, rawTime={rawTime}, formattedTime={formattedTime}");
            var result = timeFormatMode == TimeFormatMode.Raw ? rawTime : formattedTime;
            System.Diagnostics.Debug.WriteLine($"Converter result: {result}");
            return result;
        }
        
        System.Diagnostics.Debug.WriteLine($"Converter: Invalid values count={values.Count}");
        return string.Empty;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
