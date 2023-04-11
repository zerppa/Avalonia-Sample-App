namespace MyApp.UI;

using System;
using System.Globalization;

using Avalonia.Controls;
using Avalonia.Data.Converters;

/// <summary>
/// Converts any value into a <see cref="bool"/> that determines whether the Design mode is active.
/// </summary>
/// <seealso cref="Avalonia.Data.Converters.IValueConverter" />
public class ConvertDesignModeToBoolean : IValueConverter
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly ConvertDesignModeToBoolean Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Design.IsDesignMode;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}