namespace MyApp.Features.HomeScreen;

/// <summary>
/// The Home screen.
/// </summary>
/// <seealso cref="MyApp.App.Feature" />
[LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
[LocalizedResources(Culture = "fi-fi", Path = "Locales/fi-fi.axaml")]
public class HomeScreenFeature : App.Feature
{
    /// <summary>
    /// Creates the Home screen.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<HomeScreenView>("IDE.HomeScreen", Description = "The Home screen.")]
    public ViewModel? CreateHomeScreen(object? parameter)
    {
        return new HomeScreenViewModel();
    }
}