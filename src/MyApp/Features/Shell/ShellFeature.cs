namespace MyApp.Features.Shell;

using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

/// <summary>
/// The shell that hosts all other UI.
/// </summary>
/// <seealso cref="MyApp.App.Feature" />
/// <seealso cref="MyApp.Features.Shell.IShell" />
[LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
[LocalizedResources(Culture = "fi-fi", Path = "Locales/fi-fi.axaml")]
public class ShellFeature : App.Feature, IShell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellFeature"/> class.
    /// </summary>
    public ShellFeature()
    {
        if (App.Current is { } app)
        {
            app.ActualThemeVariantChanged += this.OnAppActualThemeVariantChanged;
            App.AccentColorProperty.Changed.AddClassHandler<App>((_, _) => this.OnPropertyChanged(nameof(this.AccentColor)));
            App.LanguageProperty.Changed.AddClassHandler<App>((_, _) => this.OnPropertyChanged(nameof(this.Language)));
        }
    }

    /// <inheritdoc />
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <inheritdoc />
    public event EventHandler<AccentColorChangedEventArgs>? AccentColorChanged;

    /// <inheritdoc />
    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    /// <inheritdoc />
    public Theme Theme
    {
        get => KeyToTheme(App.Current?.ActualThemeVariant.Key as string);
        set
        {
            if (App.Current is { } app && ThemeVariantToTheme(Try(() => app.ActualThemeVariant)) != value)
            {
                var old = app.ActualThemeVariant;
                app.RequestedThemeVariant = ThemeToThemeVariant(value);
                this.OnPropertyChanged();
                this.ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(ThemeVariantToTheme(old), value));
            }

            this.Host!.Settings.SetValue<string>("APP.Theme", ThemeToKey(this.Theme));

            static ThemeVariant Try(Func<ThemeVariant> getter)
            {
                try
                {
                    return getter();
                }
                catch
                {
                    return ThemeVariant.Light;
                }
            }
        }
    }

    /// <inheritdoc />
    public string AccentColor
    {
        get => ColorToString(App.Current?.AccentColor ?? Colors.Transparent);
        set
        {
            var color = StringToColor(value);
            if (App.Current is { } app && app.AccentColor != color)
            {
                var old = ColorToString(app.AccentColor);
                app.AccentColor = color;
                this.OnPropertyChanged();
                this.AccentColorChanged?.Invoke(this, new AccentColorChangedEventArgs(old, ColorToString(color)));
            }

            this.Host!.Settings.SetValue<string>("APP.AccentColor", this.AccentColor);
        }
    }

    /// <inheritdoc />
    public string? Language
    {
        get => App.Current?.Language;
        set
        {
            if (App.Current is { } app && app.Language != value)
            {
                var old = app.Language;
                app.Language = value;
                this.OnPropertyChanged();
                this.LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(old, value));
            }

            this.Host!.Settings.SetValue<string>("APP.Language", this.Language ?? App.DefaultCulture);
        }
    }

    /// <summary>
    /// Creates the shell.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<ShellView>("IDE.Shell", Description = "The root UI.")]
    public ViewModel? CreateShell(object? parameter)
    {
        return new ShellViewModel(this);
    }

    /// <inheritdoc />
    public override Task InitializeAsync()
    {
        this.Theme = KeyToTheme(this.Host!.Settings.GetValue<string?>("APP.Theme", Theme.Light.ToString()));
        this.AccentColor = this.Host!.Settings.GetValue<string?>("APP.AccentColor") ?? "#FF0078D7";
        this.Language = this.Host!.Settings.GetValue<string?>("APP.Language", App.DefaultCulture);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts <see cref="MyApp.Theme"/> into <see cref="Avalonia.Styling.ThemeVariant"/>.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <returns>The theme variant.</returns>
    private static ThemeVariant ThemeToThemeVariant(Theme theme) => theme switch
    {
        Theme.Light => ThemeVariant.Light,
        Theme.Dark => ThemeVariant.Dark,
        _ => throw new InvalidOperationException("Unrecognized theme."),
    };

    /// <summary>
    /// Converts <see cref="Avalonia.Styling.ThemeVariant"/> into <see cref="MyApp.Theme"/>.
    /// </summary>
    /// <param name="theme">The theme variant.</param>
    /// <returns>The theme.</returns>
    private static Theme ThemeVariantToTheme(ThemeVariant theme)
    {
        if (theme == ThemeVariant.Light)
        {
            return Theme.Light;
        }

        if (theme == ThemeVariant.Dark)
        {
            return Theme.Dark;
        }

        return Theme.Light;
    }

    /// <summary>
    /// Converts <see cref="MyApp.Theme"/> into <see cref="string"/>.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <returns>The theme key.</returns>
    private static string ThemeToKey(Theme theme) => theme switch
    {
        Theme.Light => "Light",
        Theme.Dark => "Dark",
        _ => throw new InvalidOperationException("Unrecognized theme."),
    };

    /// <summary>
    /// Converts <see cref="string"/> into <see cref="MyApp.Theme"/>.
    /// </summary>
    /// <param name="key">The theme key.</param>
    /// <returns>The theme.</returns>
    private static Theme KeyToTheme(string? key) => key switch
    {
        "Light" => Theme.Light,
        "Dark" => Theme.Dark,
        _ => Theme.Light,
    };

    /// <summary>
    /// Converts <see cref="Color"/> into ARGB hex <see cref="string"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The hex string.</returns>
    private static string ColorToString(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    ///  Converts ARGB hex <see cref="string"/> into <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The color, or <c>Transparent</c> if input cannot be parsed.</returns>
    private static Color StringToColor(string color)
    {
        return Color.TryParse(color, out var result) ? result : Colors.Transparent;
    }

    /// <summary>
    /// Called when the <see cref="Application.ActualThemeVariantChanged"/> event fires.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnAppActualThemeVariantChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(this.Theme));
    }
}