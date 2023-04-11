namespace MyApp.UI;

using System;
using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;

/// <summary>
/// Converts a <see cref="string"/> key into localized <see cref="string"/> value.
/// </summary>
/// <seealso cref="Avalonia.Data.Converters.IValueConverter" />
public class ConvertKeyToLocalizedString : IValueConverter
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly ConvertKeyToLocalizedString Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key)
        {
            return App.Current?.Host.UI.GetLocalizedText(key) ?? key;
        }

        return BindingOperations.DoNothing;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}