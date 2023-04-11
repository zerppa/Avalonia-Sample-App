namespace MyApp.Features.Shell;

using System;

/// <summary>
/// Holds the event arguments for the <see cref="IShell.ThemeChanged"/> event.
/// </summary>
/// <seealso cref="System.EventArgs" />
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="from">The old theme before change.</param>
    /// <param name="to">The new theme after change.</param>
    public ThemeChangedEventArgs(Theme from, Theme to)
    {
        this.From = from;
        this.To = to;
    }

    /// <summary>
    /// Gets the previously active theme.
    /// </summary>
    public Theme From { get; }

    /// <summary>
    /// Gets the newly activated theme.
    /// </summary>
    public Theme To { get; }
}