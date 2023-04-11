namespace MyApp.Features.Shell;

using System;

/// <summary>
/// Holds the event arguments for the <see cref="IShell.AccentColorChanged"/> event.
/// </summary>
/// <seealso cref="System.EventArgs" />
public class AccentColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccentColorChangedEventArgs"/> class.
    /// </summary>
    /// <param name="from">The old color before change.</param>
    /// <param name="to">The new color after change.</param>
    public AccentColorChangedEventArgs(string from, string to)
    {
        this.From = from;
        this.To = to;
    }

    /// <summary>
    /// Gets the previously active color.
    /// </summary>
    public string From { get; }

    /// <summary>
    /// Gets the newly activated color.
    /// </summary>
    public string To { get; }
}