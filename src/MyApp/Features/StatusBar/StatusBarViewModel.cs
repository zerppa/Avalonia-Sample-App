namespace MyApp.Features.StatusBar;

using System;

/// <summary>
/// View model for the Status Bar.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public sealed partial class StatusBarViewModel : ViewModel
{
    private readonly IStatusBar statusBar;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusBarViewModel"/> class.
    /// </summary>
    /// <param name="statusBar">The status bar.</param>
    public StatusBarViewModel(IStatusBar statusBar)
    {
        ArgumentNullException.ThrowIfNull(statusBar, nameof(statusBar));

        this.statusBar = statusBar;
    }
}