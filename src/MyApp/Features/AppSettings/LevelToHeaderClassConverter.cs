namespace MyApp.Features.AppSettings;

using System;
using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;

/// <summary>
/// Converts a <see cref="int"/> representing hierarchy level into the corresponding header class.
/// Integers [1..6] will produce a style names from "h1" to "h6".
/// </summary>
/// <seealso cref="Avalonia.Data.Converters.IValueConverter" />
internal class LevelToHeaderClassConverter : IValueConverter
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly LevelToHeaderClassConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int level and >= 1 and <= 6)
        {
            return "h" + level;
        }

        return BindingOperations.DoNothing;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}