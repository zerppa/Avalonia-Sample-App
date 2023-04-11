namespace MyApp.Features.Shell;

using System;

/// <summary>
/// Holds the event arguments for the <see cref="IShell.LanguageChanged"/> event.
/// </summary>
/// <seealso cref="System.EventArgs" />
public class LanguageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageChangedEventArgs"/> class.
    /// </summary>
    /// <param name="from">The old language before change.</param>
    /// <param name="to">The new language after change.</param>
    public LanguageChangedEventArgs(string? from, string? to)
    {
        this.From = from;
        this.To = to;
    }

    /// <summary>
    /// Gets the previously active language.
    /// </summary>
    public string? From { get; }

    /// <summary>
    /// Gets the newly activated language.
    /// </summary>
    public string? To { get; }
}