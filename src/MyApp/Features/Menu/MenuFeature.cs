namespace MyApp.Features.Menu
{
    /// <summary>
    /// The main menu.
    /// </summary>
    /// <seealso cref="MyApp.App.Feature" />
    /// <seealso cref="MyApp.Features.Menu.IMenu" />
    [LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
    [LocalizedResources(Culture = "fi-fi", Path = "Locales/fi-fi.axaml")]
    public class MenuFeature : App.Feature, IMenu
    {
        /// <summary>
        /// Creates the menu.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The view model.</returns>
        [PublishedView<MenuView>("IDE.MainMenu", Description = "The main menu for the main window.")]
        public ViewModel? CreateMenu(object? parameter)
        {
            return new MenuViewModel(this);
        }
    }
}
