namespace MyApp.Features.ViewSelector;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

using MyApp.Features.Shell;

using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;

/// <summary>
/// The view selector.
/// </summary>
/// <seealso cref="MyApp.App.Feature" />
/// <seealso cref="MyApp.Features.ViewSelector.IViewSelector" />
[LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
[LocalizedResources(Culture = "fi-fi", Path = "Locales/fi-fi.axaml")]
public class ViewSelectorFeature : App.Feature, IViewSelector
{
    private static readonly IImmutableSolidColorBrush SelectorButtonLightThemeBackground = new ImmutableSolidColorBrush(Colors.White, 0.8);
    private static readonly IImmutableSolidColorBrush SelectorButtonDarkThemeBackground = new ImmutableSolidColorBrush(Color.Parse("#EE222222"));

    private readonly Dictionary<IViewItem, ViewInfo> views = new();
    private readonly ObservableCollection<ProjectItem> projects = new();

    private IViewItem currentView = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewSelectorFeature"/> class.
    /// </summary>
    public ViewSelectorFeature()
    {
        this.Projects = new ReadOnlyObservableCollection<ProjectItem>(this.projects);
    }

    /// <inheritdoc />
    public event EventHandler<ViewChangedEventArgs>? ViewChanged;

    /// <inheritdoc />
    public ReadOnlyObservableCollection<ProjectItem> Projects { get; }

    /// <summary>
    /// Gets or sets the shell.
    /// </summary>
    [Dependency]
    public IShell? Shell { get; set; }

    /// <summary>
    /// Gets the current view.
    /// </summary>
    public IViewItem CurrentView
    {
        get => this.currentView;
        private set
        {
            var old = this.currentView;
            if (old != value)
            {
                this.currentView = value;
                this.ViewChanged?.Invoke(this, new ViewChangedEventArgs(old, value));
            }
        }
    }

    /// <inheritdoc />
    IViewItem IViewSelector.CurrentView => this.CurrentView;

    /// <summary>
    /// Creates the view selector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<ViewSelectorView>("IDE.ViewSelector", Description = "The main navigation for project views.")]
    public ViewModel? CreateViewSelector(object? parameter)
    {
        return new ViewSelectorViewModel(this);
    }

    /// <inheritdoc />
    public void ActivateHome() => this.ActivateView(HomeItem.Instance);

    /// <inheritdoc />
    public void ActivateProject(ProjectItem item) => this.ActivateView(item);

    /// <inheritdoc />
    public void ActivateSettings() => this.ActivateView(SettingsItem.Instance);

    /// <inheritdoc />
    public void AddProject(ProjectItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        if (!this.projects.Contains(item))
        {
            this.views.Add(item, new()
            {
                ViewName = item.ProjectType switch
                {
                    ProjectType.ProjectType1 => "PROJECT.MyProjectType.Main",
                    _ => string.Empty
                },
                ViewParameter = item,
            });
            this.projects.Add(item);
        }
    }

    /// <inheritdoc />
    public void RemoveProject(ProjectItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        var container = this.Host?.GetInstance<IViewContainer>();
        if (container == null)
        {
            Trace.TraceWarning($"No active {nameof(IViewContainer)}.");
        }

        if (this.views.TryGetValue(item, out var info))
        {
            container?.Remove(info.RootView);

            this.projects.Remove(item);

            if (item == this.CurrentView)
            {
                this.ActivateHome();
            }
        }
        else
        {
            Trace.TraceWarning("Cannot remove project because it does not exist.");
        }
    }

    /// <inheritdoc />
    public override Task InitializeAsync()
    {
        if (this.Shell is null)
        {
            Trace.TraceError($"{nameof(ViewSelectorFeature)}: Required dependency {nameof(IShell)} could not be resolved. Feature may not work properly.");
        }
        else
        {
            this.Shell.ThemeChanged += this.OnShellThemeChanged;
        }

        // The standard in-built actions
        this.views.Add(HomeItem.Instance, new() { ViewName = "IDE.HomeScreen", ViewParameter = default });
        this.views.Add(SettingsItem.Instance, new() { ViewName = "IDE.AppSettings", ViewParameter = default });

        // The project items
        // TODO: Load from disk (the "Most Recently Used" list)
        for (var i = 0; i < 5; i++)
        {
            this.AddProject(new ProjectItem { Title = "Example", Path = $"C:\\Temp\\{i}", ProjectType = ProjectType.ProjectType1, Icon = default });
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task RunningAsync(string[] args)
    {
        if (this.Shell is not null)
        {
            this.UpdateThemeResources(this.Shell.Theme);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override async Task ExitRequestedAsync(CancellationTokenSource cancel)
    {
        var dirtyProjects = this.Projects.Where(item => item.IsDirty).ToArray();
        if (!dirtyProjects.Any())
        {
            return;
        }

        var title = this.Host!.UI.GetLocalizedText<ViewSelectorFeature>("ViewSelector.Exit");
        var header = this.Host!.UI.GetLocalizedText<ViewSelectorFeature>("ViewSelector.ExitPrompt");
        var yesButton = this.Host!.UI.GetLocalizedText("Yes");
        var noButton = this.Host!.UI.GetLocalizedText("No");
        var cancelButton = this.Host!.UI.GetLocalizedText("Cancel");

        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
            new MessageBoxCustomParams
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                MinWidth = 200,
                MaxWidth = 400,
                ButtonDefinitions = new[]
                {
                    new ButtonDefinition { Name = yesButton, IsDefault = true },
                    new ButtonDefinition { Name = noButton },
                    new ButtonDefinition { Name = cancelButton, IsCancel = true }
                },
                ContentTitle = title,
                ContentHeader = string.Format(header ?? string.Empty, dirtyProjects.Length),
                ContentMessage = string.Join('\n', dirtyProjects.Select(project => $"⚠️ {project.Title}")),
                /*WindowIcon = new WindowIcon("avalonia-logo.ico")*/
            });
        var result = await messageBoxStandardWindow.ShowDialog(App.Current!.MainWindow);
        if (result == cancelButton)
        {
            cancel.Cancel();
        }
        else if (result == yesButton)
        {
            var saveTasks = this.Projects
                .Where(item => item.IsDirty && item.SaveAsync is not null)
                .Select(item => item.SaveAsync!())
                .Where(task => task is not null)
                .ToArray();

            await Task.WhenAll(saveTasks);
        }
    }

    /// <summary>
    /// Called when the <see cref="IShell.ThemeChanged"/> event fires.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ThemeChangedEventArgs"/> instance containing the event data.</param>
    private void OnShellThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        this.UpdateThemeResources(e.To);
    }

    /// <summary>
    /// Updates the theme resources.
    /// </summary>
    /// <param name="theme">The theme.</param>
    private void UpdateThemeResources(Theme theme)
    {
        if (App.Current?.MainWindow?.Resources is { } resources)
        {
            switch (theme)
            {
                case Theme.Light:
                    resources["ViewSelector.SelectorButton.Background"] = SelectorButtonLightThemeBackground;
                    break;
                case Theme.Dark:
                    resources["ViewSelector.SelectorButton.Background"] = SelectorButtonDarkThemeBackground;
                    break;
                default:
                    resources["ViewSelector.SelectorButton.Background"] = null;
                    break;
            }
        }
    }

    /// <summary>
    /// Activates the view.
    /// </summary>
    /// <param name="item">The item.</param>
    private void ActivateView(IViewItem item)
    {
        if (item is null)
        {
            return;
        }

        var container = this.Host?.GetInstance<IViewContainer>();
        if (container == null)
        {
            Trace.TraceWarning($"No active {nameof(IViewContainer)}.");
        }

        if (this.views.TryGetValue(item, out var info))
        {
            if (!info.IsLoaded)
            {
                info.RootView = App.Current?.CreateView(info.ViewName, info.ViewParameter);
                info.IsLoaded = true;
            }

            container?.AddOrActivate(info.RootView);

            this.CurrentView = item;
        }
        else
        {
            Trace.TraceWarning("Cannot activate view because it has been removed.");
        }
    }

    /// <summary>
    /// Represents the special Home item in the view list.
    /// </summary>
    /// <seealso cref="MyApp.Features.ViewSelector.IViewItem" />
    public sealed class HomeItem : IViewItem
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="HomeItem"/> class from being created.
        /// </summary>
        private HomeItem()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static HomeItem Instance { get; } = new();
    }

    /// <summary>
    /// Represents the special Settings item in the view list.
    /// </summary>
    /// <seealso cref="MyApp.Features.ViewSelector.IViewItem" />
    public sealed class SettingsItem : IViewItem
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="SettingsItem"/> class from being created.
        /// </summary>
        private SettingsItem()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SettingsItem Instance { get; } = new();
    }

    /// <summary>
    /// Extra data on available project items.
    /// </summary>
    private class ViewInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether the project's view has been created.
        /// </summary>
        internal bool IsLoaded { get; set; }

        /// <summary>
        /// Gets the published view's name.
        /// </summary>
        internal required string ViewName { get; init; }

        /// <summary>
        /// Gets the published view's parameter.
        /// </summary>
        internal required object? ViewParameter { get; init; }

        /// <summary>
        /// Gets or sets the view instance.
        /// </summary>
        /// <remarks>Usually <c>null</c> until the view has been loaded (in which case <see cref="IsLoaded"/> will also be <c>true</c>).</remarks>
        internal Control? RootView { get; set; }
    }
}