namespace MyApp.Features.Menu;

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.Input;

/// <summary>
/// The view model for Menu.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class MenuViewModel : ViewModel
{
    private readonly IMenu menu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuViewModel"/> class.
    /// </summary>
    /// <param name="menu">The menu.</param>
    public MenuViewModel(IMenu menu)
    {
        this.menu = menu;

        this.Items.Add("One");
        this.Items.Add("Two");
        this.Items.Add("Three");
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public ObservableCollection<string> Items { get; } = new();

    /// <summary>
    /// Attempts to exit the application.
    /// </summary>
    [RelayCommand]
    public void Exit()
    {
        App.Current?.Exit();
    }
}