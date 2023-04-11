namespace MyApp.Features.HomeScreen;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using CommunityToolkit.Mvvm.Input;

using MyApp.Features.Shell;

/// <summary>
/// The view model for Home screen.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class HomeScreenViewModel : ViewModel
{
    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private LanguageViewModel? selectedLanguage;

    /// <summary>
    /// Gets or sets the shell.
    /// </summary>
    [Dependency]
    public IShell? Shell { get; set; }

    /// <summary>
    /// Gets the languages.
    /// </summary>
    public ObservableCollection<LanguageViewModel> Languages { get; } = new();

    /// <summary>
    /// Gets or sets the selected language.
    /// </summary>
    public LanguageViewModel? SelectedLanguage
    {
        get => this.selectedLanguage;
        set
        {
            if (this.Shell is { } shell && this.SetProperty(ref this.selectedLanguage, value))
            {
                shell.Language = value?.Identifier;
            }
        }
    }

    /// <inheritdoc />
    protected override void Initialize()
    {
        if (this.Shell is null)
        {
            Trace.TraceError($"{nameof(HomeScreenViewModel)}: Required dependency {nameof(IShell)} could not be resolved. Feature may not work properly.");
        }
        else
        {
            this.Languages.Add(new() { LocalizedName = "English", Identifier = App.DefaultCulture });
            this.Languages.Add(new() { LocalizedName = "Suomi", Icon = "Flag_Finland", Identifier = "fi-fi" });

            this.SelectedLanguage = this.Languages.FirstOrDefault(lang => lang.Identifier == this.Shell.Language);
        }
    }

    /// <summary>
    /// Changes the theme.
    /// </summary>
    /// <param name="theme">The theme.</param>
    [RelayCommand]
    private void ChangeTheme(Theme theme)
    {
        if (this.Shell is { } shell)
        {
            shell.Theme = theme;
        }
    }

    /// <summary>
    /// Changes the accent color.
    /// </summary>
    /// <param name="accent">The accent.</param>
    [RelayCommand]
    private void ChangeAccent(string accent)
    {
        if (this.Shell is { } shell)
        {
            shell.AccentColor = accent;
        }
    }
}