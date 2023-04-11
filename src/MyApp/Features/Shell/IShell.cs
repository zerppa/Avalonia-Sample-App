namespace MyApp.Features.Shell;

using System;

/// <summary>
/// Controls the Shell.
/// </summary>
public interface IShell
{
    /// <summary>
    /// Occurs when the theme has changed.
    /// </summary>
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    /// <summary>
    /// Occurs when the accent color has changed.
    /// </summary>
    event EventHandler<AccentColorChangedEventArgs> AccentColorChanged;

    /// <summary>
    /// Occurs when the language has changed.
    /// </summary>
    event EventHandler<LanguageChangedEventArgs> LanguageChanged;

    /// <summary>
    /// Gets or sets the theme.
    /// </summary>
    Theme Theme { get; set; }

    /// <summary>
    /// Gets or sets the accent color as hex using ARGB components.
    /// The default is <c>#FF0078D7</c>.
    /// </summary>
    string AccentColor { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    /// <remarks>
    /// Example value: <c>"en-us"</c>.
    /// </remarks>
    string? Language { get; set; }
}