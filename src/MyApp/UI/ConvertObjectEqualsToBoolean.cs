namespace MyApp.UI;

using System;
using System.Globalization;

using Avalonia.Data.Converters;

/// <summary>
/// Determines whether the object and parameter have the equal value.
/// </summary>
/// <seealso cref="Avalonia.Data.Converters.IValueConverter" />
public class ConvertObjectEqualsToBoolean : IValueConverter
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly ConvertObjectEqualsToBoolean Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null && parameter is null)
        {
            return true;
        }

        return value is not null && value.Equals(parameter);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}