namespace MyApp.Features.StatusBar;

/// <summary>
/// The status bar.
/// </summary>
/// <seealso cref="MyApp.App.Feature" />
/// <seealso cref="MyApp.Features.StatusBar.IStatusBar" />
public class StatusBarFeature : App.Feature, IStatusBar
{
    /// <summary>
    /// Creates the status bar.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<StatusBarView>("IDE.StatusBar", Description = "The status bar.")]
    public ViewModel? CreateStatusBar(object? parameter)
    {
        return new StatusBarViewModel(this);
    }
}