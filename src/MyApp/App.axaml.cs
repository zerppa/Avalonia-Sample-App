namespace MyApp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

using MyApp.Features.AppSettings;
using MyApp.Features.HomeScreen;
using MyApp.Features.Menu;
using MyApp.Features.Project;
using MyApp.Features.Shell;
using MyApp.Features.StatusBar;
using MyApp.Features.ViewSelector;
using MyApp.Testing.CustomStartup;

/// <summary>
/// The application.
/// </summary>
/// <seealso cref="Avalonia.Application" />
/// <seealso cref="MyApp.IHost" />
public sealed partial class App : Application, IHost
{
    /// <summary>
    /// The default culture for localized content.
    /// </summary>
    public const string DefaultCulture = "en-us";

    /// <summary>
    /// Defines the <see cref="AccentColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> AccentColorProperty =
        AvaloniaProperty.Register<App, Color>(
            nameof(AccentColor),
            defaultValue: Color.Parse("#FF0078D7"));

    /// <summary>
    /// Defines the <see cref="Language"/> property.
    /// </summary>
    public static readonly DirectProperty<App, string?> LanguageProperty =
        AvaloniaProperty.RegisterDirect<App, string?>(
            nameof(Language),
            o => o.Language,
            (o, v) => o.Language = v,
            DefaultCulture);

    private readonly List<Feature> features = new();
    private readonly Dictionary<string, PublishedViewInfo> publishedViews = new();
    private readonly Dictionary<Control, ActiveView> activeViews = new();
    private readonly Dictionary<Feature, Dictionary<string, ResourceDictionary>> localizedResources = new();
    private readonly Dictionary<string, string?> coalescedStrings = new();

    private bool appExiting;

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private string? language = DefaultCulture.ToLowerInvariant();

    /// <summary>
    /// Initializes static members of the <see cref="App"/> class.
    /// </summary>
    static App()
    {
        AccentColorProperty.Changed.AddClassHandler<App>((x, e) => x.OnAccentPropertyChanged(e));
        LanguageProperty.Changed.AddClassHandler<App>((x, e) => x.OnLanguageChanged(e));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Only one App instance is allowed in an application.</exception>
    public App()
    {
        if (Current is not null && !Design.IsDesignMode)
        {
            throw new InvalidOperationException("Only one App instance is allowed in an application.");
        }

        Current = this;
    }

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    /// <value>The current application instance.</value>
    public static new App? Current { get; private set; }

    /// <summary>
    /// Gets the application common services.
    /// </summary>
    public IHost Host => this;

    /// <summary>
    /// Gets the loaded Feature instances.
    /// </summary>
    public IReadOnlyCollection<Feature> Features => this.features;

    /// <summary>
    /// Gets the main window.
    /// </summary>
    public Window? MainWindow { get; private set; }

    /// <summary>
    /// Gets or sets the accent color.
    /// </summary>
    public Color AccentColor
    {
        get => this.GetValue(AccentColorProperty);
        set => this.SetValue(AccentColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    /// <value>
    /// The language identifier. Value for English (which is also the default) is <c>en-us</c>.
    /// Assigning <c>null</c> will use the default language (English) instead.
    /// This property never returns <c>null</c>.
    /// </value>
    /// <remarks>
    /// See also <see cref="DefaultCulture"/>.
    /// </remarks>
    public string? Language
    {
#pragma warning disable CS8766
        get => this.language ?? DefaultCulture;
#pragma warning restore CS8766
        set => this.SetAndRaise(LanguageProperty, ref this.language, (value ?? DefaultCulture).ToLowerInvariant());
    }

    /// <summary>
    /// Gets the associated <see cref="IApplicationLifetime"/> instance.
    /// </summary>
    private IClassicDesktopStyleApplicationLifetime? Lifetime => (IClassicDesktopStyleApplicationLifetime?)this.ApplicationLifetime;

    /// <summary>
    /// Creates the Published View.
    /// </summary>
    /// <param name="viewName">The view name.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view instance, or <c>null</c> if no Published View was registered for the specified name.</returns>
    public Control? CreateView(string viewName, object? parameter)
    {
        if (this.publishedViews.TryGetValue(viewName, out var info))
        {
            return info.CreateView(parameter);
        }

        Trace.TraceWarning($"Could not find Published View '{viewName}'.");

        return null;

        // return new TextBlock { Text = $"View not found. ViewName:{viewName}; ViewParameter:{viewParameter}", Background = Brushes.PaleGoldenrod };
    }

    /// <summary>
    /// Requests application exit.
    /// </summary>
    /// <remarks>
    /// Acts as an event handler, thus <c>async void</c>.
    /// </remarks>
    public async void Exit() => await this.Exit(false);

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktopLifetime.ShutdownRequested += async (_, e) =>
            {
                e.Cancel = true;
                if (await this.ExitRequestedAsync())
                {
                    await this.Exit(true);
                }
            };

            _ = this.LaunchAppAsync(desktopLifetime);
        }
        else if (!Design.IsDesignMode)
        {
            throw new NotSupportedException($"{nameof(this.ApplicationLifetime)} must be of type {nameof(IClassicDesktopStyleApplicationLifetime)}");
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Configures the view.
    /// </summary>
    /// <param name="source">The source feature.</param>
    /// <param name="viewName">The view name.</param>
    /// <param name="view">The view.</param>
    /// <param name="viewModel">The view model.</param>
    /// <remarks>
    /// Initializes the view (using the current localization) and its data context.
    /// Updates the <see cref="activeViews"/> list and sets up the view model lifetime events.
    /// </remarks>
    internal void ConfigureView(Feature source, string viewName, Control view, ViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(viewName, nameof(viewName));
        ArgumentNullException.ThrowIfNull(view, nameof(view));
        ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));

        this.LocalizeView(view, source);

        this.ResolveDependencies(viewModel);
        viewModel.Host = this;
        view.DataContext = viewModel;

        EventHandler<LogicalTreeAttachmentEventArgs> attached = (_, _) =>
        {
            this.activeViews.Add(view, new(source, viewName, view, viewModel));
            Debug.WriteLine($"New Published view activated: {viewName}");
            viewModel.Activate();
        };

        EventHandler<LogicalTreeAttachmentEventArgs> detached = (_, _) =>
        {
            viewModel.Deactivate();
            this.activeViews.Remove(view, out var entry);
            Debug.WriteLine($"Published view deactivated: {entry!.ViewName}");
        };

        EventHandler<RoutedEventArgs> unloaded = default!;
        unloaded = (_, _) =>
        {
            view.AttachedToLogicalTree -= attached;
            view.DetachedFromLogicalTree -= detached;
            view.Unloaded -= unloaded;
        };

        view.Unloaded += unloaded;
        view.AttachedToLogicalTree += attached;
        view.DetachedFromLogicalTree += detached;
    }

    /// <summary>
    /// Launches the application and executes the needed startup actions.
    /// </summary>
    /// <param name="lifetime">The <see cref="IApplicationLifetime"/>.</param>
    private async Task LaunchAppAsync(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(lifetime, nameof(lifetime));

        await this.LoadSettings();

        // Load Features
        this.features.AddRange(this.LoadFeatures());

        // Load Published Views
        foreach (var publishedView in this.LoadPublishedViews())
        {
            this.publishedViews.Add(publishedView.ViewName, publishedView);
        }

        // Load Resources
        foreach (var feature in this.features)
        {
            this.LoadResources(feature);
        }

        // Resolve Dependencies
        foreach (var feature in this.features)
        {
            this.ResolveDependencies(feature);
        }

        await this.InitializeAsync();
        if (!await this.StartingAsync())
        {
            await this.Exit(true);

            Trace.TraceInformation("Application exiting early because of some features.");
            Environment.Exit(default);
        }

        var mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        mainWindow.Closing += (_, e) =>
        {
            if (this.appExiting)
            {
                return;
            }

            e.Cancel = true;
            lifetime.TryShutdown();
        };
        mainWindow.Closed += async (_, _) =>
        {
            this.MainWindow = null;
            await this.Exit(true);
        };

        lifetime.MainWindow = this.MainWindow = mainWindow;
        mainWindow.Show();

        await this.RunningAsync(lifetime.Args ?? Array.Empty<string>());
    }

    /// <summary>
    /// Begins the application shutdown sequence.
    /// </summary>
    /// <param name="force"><c>true</c> to attempt bypass window closing prompt, false to attempt closing main window with the option to cancel.</param>
    private async Task Exit(bool force)
    {
        if (this.appExiting || this.Lifetime is null)
        {
            return;
        }

        if (force)
        {
            this.appExiting = true;
            await this.ShutdownAsync();
            this.Lifetime.Shutdown();
        }
        else
        {
            this.MainWindow?.Close();
        }
    }

    /// <summary>
    /// Loads the features.
    /// </summary>
    /// <returns>Collection of Feature instances.</returns>
    private IEnumerable<Feature> LoadFeatures()
    {
        yield return new ShellFeature();
        yield return new MenuFeature();
        yield return new ViewSelectorFeature();
        yield return new StatusBarFeature();
        yield return new HomeScreenFeature();
        yield return new AppSettingsFeature();
        yield return new ProjectFeature();

        yield return new CustomStartupFeature(); // TODO: This is mainly used for automation at start-up. Remove me
    }

    /// <summary>
    /// Loads the published views.
    /// </summary>
    /// <returns>Collection of Published View instantiation infos.</returns>
    private IEnumerable<PublishedViewInfo> LoadPublishedViews()
    {
        return this.Features.SelectMany(this.LoadPublishedViews);
    }

    /// <summary>
    /// Loads the published views.
    /// </summary>
    /// <param name="instance">The instance from which to scan for Published View definitions.</param>
    /// <returns>Collection of Published View instantiation infos.</returns>
    private IEnumerable<PublishedViewInfo> LoadPublishedViews(Feature instance)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));

        var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var publishedViewInfos = from attribute in method.GetCustomAttributes(typeof(PublishedViewAttribute<>)).OfType<IPublishedViewInfo>()
                let factoryMethod = Delegate.CreateDelegate(typeof(Func<object, ViewModel>), instance, method, false) as Func<object, ViewModel>
                where factoryMethod is not null
                select new PublishedViewInfo(instance, factoryMethod, attribute);

            foreach (var info in publishedViewInfos)
            {
                yield return info;
            }
        }
    }

    /// <summary>
    /// Loads the <see cref="ResourceDictionary"/> declared by features.
    /// </summary>
    /// <param name="instance">The owner feature.</param>
    private void LoadResources(Feature instance)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));

        var type = instance.GetType();
        var assemblyName = type.Assembly.GetName().Name!;
        var ns = type.Namespace!;
        if (ns.StartsWith(assemblyName + "."))
        {
            var path = ns[assemblyName.Length..].Replace('.', '/');

            // Localized resources
            foreach (var attribute in type.GetCustomAttributes<LocalizedResourcesAttribute>().Where(a => !string.IsNullOrWhiteSpace(a.Path)))
            {
                try
                {
                    var relativePath = attribute.Path.Trim().Trim('/');
                    var culture = attribute.Culture?.ToLower() ?? DefaultCulture;

                    if (!relativePath.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
                    {
                        Trace.TraceInformation($"Skipping localized resource '{relativePath}' for feature '{type.FullName}' because its path does not point to an .axaml file.");
                        continue;
                    }

                    if (AvaloniaXamlLoader.Load(new Uri($"avares://{assemblyName}{path}/{relativePath}")) is ResourceDictionary res)
                    {
                        if (!this.localizedResources.TryGetValue(instance, out var featureLocalization))
                        {
                            featureLocalization = this.localizedResources[instance] = new();
                        }

                        featureLocalization[culture] = res;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error loading a localized resource for feature '{type.FullName}'. {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Resolves the dependencies of the target.
    /// </summary>
    /// <param name="target">The target.</param>
    private void ResolveDependencies(object target)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));

        var sources = new object[] { this }.Concat(this.features).ToArray();

        var dependencies = from property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           where property.CanWrite && property.GetCustomAttribute<DependencyAttribute>() is not null
                           select property;

        foreach (var dependency in dependencies)
        {
            foreach (var source in sources)
            {
                if (dependency.PropertyType.IsInstanceOfType(source))
                {
                    dependency.SetValue(target, source);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Applies the localized resources on the view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="owner">The owner feature.</param>
    private void LocalizeView(Control view, Feature owner)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));
        ArgumentNullException.ThrowIfNull(owner, nameof(owner));

        if (this.localizedResources.TryGetValue(owner, out var localizations) && localizations.Any())
        {
            localizations.TryGetValue(DefaultCulture, out var defaultResources);
            localizations.TryGetValue(this.Language!, out var currentResources);

            // Remove all non-English resources
            foreach (var toRemove in localizations.Values.Where(loc => loc != defaultResources))
            {
                view.Resources.MergedDictionaries.Remove(toRemove);
            }

            // Add the English resources if not yet present
            if (defaultResources is not null && !view.Resources.MergedDictionaries.Contains(defaultResources))
            {
                view.Resources.MergedDictionaries.Insert(0, defaultResources);
            }

            // Add the current culture resources if not yet present
            if (currentResources is not null && !view.Resources.MergedDictionaries.Contains(currentResources))
            {
                view.Resources.MergedDictionaries.Add(currentResources);
            }

            // Inject to the coalesced master dictionary
            // Note: We're calling ResourceDictionary.TryGetValue instead of just accessing pair.Value because of Avalonia's ResourceDictionary.DeferredItem objects.
            // See https://github.com/AvaloniaUI/Avalonia/issues/8790 for more details
            foreach (var pair in defaultResources ?? Enumerable.Empty<KeyValuePair<object, object?>>())
            {
                if (pair.Key is string key && defaultResources!.TryGetValue(pair.Key, out var materialized) && materialized is string value)
                {
                    this.coalescedStrings[key] = value;
                }
            }

            foreach (var pair in currentResources ?? Enumerable.Empty<KeyValuePair<object, object?>>())
            {
                if (pair.Key is string key && currentResources!.TryGetValue(pair.Key, out var materialized) && materialized is string value)
                {
                    this.coalescedStrings[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Initializes the Features.
    /// </summary>
    private async Task InitializeAsync()
    {
        Trace.TraceInformation($"Features: {nameof(this.InitializeAsync)}");
        var tasks = this.features.Select(f => f.InitializeAsync()).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Signals the Features that they should execute their startup actions.
    /// </summary>
    private async Task<bool> StartingAsync()
    {
        Trace.TraceInformation($"Features: {nameof(this.StartingAsync)}");
        using var cancellation = new CancellationTokenSource();

        // NOTE: Call in sequence (instead of parallel) because their StartingAsync are expected to register functionality using other features' API,
        // and we want ordering to matter and be deterministic (parallel tasks will run and complete somewhat randomly)
        foreach (Feature startingFeature in this.features)
        {
            await startingFeature.StartingAsync(cancellation);
        }

        return !cancellation.IsCancellationRequested;
    }

    /// <summary>
    /// Signals the Features that the application is fully initialized and running.
    /// </summary>
    /// <param name="args">The commandline arguments.</param>
    private async Task RunningAsync(string[] args)
    {
        Trace.TraceInformation($"Features: {nameof(this.RunningAsync)}");
        var tasks = this.features.Select(f => f.RunningAsync(args ?? Array.Empty<string>())).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Signals the Features that user has requested exit. A feature may cancel the shutdown.
    /// </summary>
    /// <returns><c>true</c> if shutdown is allowed i.e. no cancellation was requested.</returns>
    private async Task<bool> ExitRequestedAsync()
    {
        Trace.TraceInformation($"Features: {nameof(this.ExitRequestedAsync)}");
        using var cancellation = new CancellationTokenSource();
        var tasks = this.features.Select(f => f.ExitRequestedAsync(cancellation)).ToArray();
        await Task.WhenAll(tasks);
        return !cancellation.IsCancellationRequested;
    }

    /// <summary>
    /// Signals the Features that they should execute their shutdown actions.
    /// </summary>
    private async Task ShutdownAsync()
    {
        Trace.TraceInformation($"Features: {nameof(this.ShutdownAsync)}");
        var tasks = this.features.Select(f => f.ShutdownAsync()).ToArray();
        await Task.WhenAll(tasks);

        await this.SaveSettings();
    }

    /// <summary>
    /// Saves the settings.
    /// </summary>
    private async Task SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(this.Host.UserSettingsPath);
            var settingsFile = Path.Combine(this.Host.UserSettingsPath, "settings.json");
            await using var stream = File.OpenWrite(settingsFile);
            await JsonSerializer.SerializeAsync(stream, this.settings);
        }
        catch (Exception e)
        {
            Trace.TraceError($"{nameof(this.SaveSettings)}: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private async Task LoadSettings()
    {
        try
        {
            Directory.CreateDirectory(this.Host.UserSettingsPath);
            var settingsFile = Path.Combine(this.Host.UserSettingsPath, "settings.json");
            await using var stream = File.OpenRead(settingsFile);
            var deserialized = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
            if (deserialized is null)
            {
                Trace.TraceInformation($"{nameof(this.LoadSettings)}: No saved settings.");
                return;
            }

            foreach (var pair in deserialized)
            {
                this.settings[pair.Key] = pair.Value;
            }
        }
        catch (Exception e)
        {
            Trace.TraceError($"{nameof(this.LoadSettings)}: {e.Message}");
        }
    }

    /// <summary>
    /// Called when the <see cref="AccentColor"/> changes.
    /// </summary>
    /// <param name="e">The <see cref="Avalonia.AvaloniaPropertyChangedEventArgs"/> instance containing the event data.</param>
    private void OnAccentPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var baseAccent = (Color)e.NewValue!;

        this.Resources["SystemAccentColor"] = baseAccent;
        this.Resources["SystemAccentColorDark1"] = ChangeColorLuminosity(baseAccent, -0.3);
        this.Resources["SystemAccentColorDark2"] = ChangeColorLuminosity(baseAccent, -0.5);
        this.Resources["SystemAccentColorDark3"] = ChangeColorLuminosity(baseAccent, -0.7);
        this.Resources["SystemAccentColorLight1"] = ChangeColorLuminosity(baseAccent, 0.3);
        this.Resources["SystemAccentColorLight2"] = ChangeColorLuminosity(baseAccent, 0.5);
        this.Resources["SystemAccentColorLight3"] = ChangeColorLuminosity(baseAccent, 0.7);

        static Color ChangeColorLuminosity(Color color, double luminosityFactor)
        {
            var red = (double)color.R;
            var green = (double)color.G;
            var blue = (double)color.B;

            if (luminosityFactor < 0)
            {
                luminosityFactor = 1 + luminosityFactor;
                red *= luminosityFactor;
                green *= luminosityFactor;
                blue *= luminosityFactor;
            }
            else if (luminosityFactor >= 0)
            {
                red = (255 - red) * luminosityFactor + red;
                green = (255 - green) * luminosityFactor + green;
                blue = (255 - blue) * luminosityFactor + blue;
            }

            return new Color(color.A, (byte)red, (byte)green, (byte)blue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Language"/> changes.
    /// </summary>
    /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs"/> instance containing the event data.</param>
    private void OnLanguageChanged(AvaloniaPropertyChangedEventArgs e)
    {
        foreach (var viewInfo in this.activeViews.Values)
        {
            this.LocalizeView(viewInfo.View, viewInfo.Owner);
        }

        // HACK: Force DynamicResources to refresh
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (this.MainWindow is { } mainWindow)
            {
                var original = mainWindow.FontSize;
                mainWindow.FontSize -= 0.001;
                mainWindow.FontSize = original;
            }
        });
    }

    /// <summary>
    /// Data entry for <see cref="activeViews"/>.
    /// </summary>
    private record ActiveView(Feature Owner, string ViewName, Control View, ViewModel ViewModel);
}