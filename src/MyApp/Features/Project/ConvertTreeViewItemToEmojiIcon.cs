namespace MyApp.Features.Project;

using System;
using System.Globalization;

using Avalonia.Controls;
using Avalonia.Data.Converters;

/// <summary>
/// Picks an appropriate icon for a <see cref="TreeViewItem"/> according to whether it has child nodes.
/// </summary>
/// <seealso cref="Avalonia.Data.Converters.IValueConverter" />
public class ConvertTreeViewItemToEmojiIcon : IValueConverter
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly ConvertTreeViewItemToEmojiIcon Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TreeViewItem item)
        {
            return item.ItemCount == 0 ? "📄" : "📁";
        }

        if (value is int integer)
        {
            return integer <= 0 ? "📄" : "📁";
        }

        return "❔";
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
