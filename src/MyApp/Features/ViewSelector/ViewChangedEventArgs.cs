namespace MyApp.Features.ViewSelector;

using System;

/// <summary>
/// Holds the event arguments for the <see cref="IViewSelector.ViewChanged"/> event.
/// </summary>
/// <seealso cref="System.EventArgs" />
public class ViewChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewChangedEventArgs"/> class.
    /// </summary>
    /// <param name="from">The old view before navigation.</param>
    /// <param name="to">The new view after navigation.</param>
    public ViewChangedEventArgs(IViewItem from, IViewItem to)
    {
        this.From = from;
        this.To = to;
    }

    /// <summary>
    /// Gets the previously active view.
    /// </summary>
    public IViewItem From { get; }

    /// <summary>
    /// Gets the newly activated view.
    /// </summary>
    public IViewItem To { get;  }
}