namespace MyApp.Features.AppSettings;

using System;
using System.Collections.ObjectModel;
using System.Linq;

using AvaloniaEdit.Utils;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MyApp.Extensions;
using MyApp.Features.Shell;

/// <summary>
/// The view model for app settings.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class AppSettingsViewModel : ViewModel
{
    private readonly IAppSettings appSettings;

    [ObservableProperty]
    private CategoryViewModel? scrollIntoViewItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsViewModel"/> class.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    public AppSettingsViewModel(IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings, nameof(appSettings));

        this.appSettings = appSettings;

        // Make view models
        this.Categories.AddRange(this.appSettings.Categories.Select(c => new CategoryViewModel(c)));
    }

    /// <summary>
    /// Gets or sets the shell.
    /// </summary>
    [Dependency]
    public IShell? Shell { get; set; }

    /// <summary>
    /// Gets the root categories.
    /// </summary>
    public ObservableCollection<CategoryViewModel> Categories { get; } = new();

    /// <inheritdoc />
    public override void Activate()
    {
        if (this.Shell is { } shell)
        {
            shell.LanguageChanged += this.OnLanguageChanged;
        }
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        if (this.Shell is { } shell)
        {
            shell.LanguageChanged -= this.OnLanguageChanged;
        }
    }

    /// <summary>
    /// Notifies the View who will then find the right element from the Visual Tree and brings it into view.
    /// </summary>
    /// <param name="target">The target.</param>
    [RelayCommand]
    public void Navigate(CategoryViewModel target)
    {
        this.ScrollIntoViewItem = null;
        this.ScrollIntoViewItem = target;
    }

    /// <summary>
    /// Called when the <see cref="IShell.LanguageChanged"/> event fires.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="LanguageChangedEventArgs"/> instance containing the event data.</param>
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        this.Categories.ForEach(c => c.Refresh());
    }
}