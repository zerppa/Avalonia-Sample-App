namespace MyApp.Features.ViewSelector;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Platform.Storage;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.Input;

using MyApp.Extensions;
using MyApp.Features.Project;

/// <summary>
/// The view model for View Selector.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public sealed partial class ViewSelectorViewModel : ViewModel
{
    private readonly IViewSelector viewSelector;
    private readonly ObservableCollection<ProjectItemViewModel> projects = new();

    private IStorageFolder? lastSelectedDirectory = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewSelectorViewModel"/> class.
    /// </summary>
    /// <param name="viewSelector">The view selector.</param>
    public ViewSelectorViewModel(IViewSelector viewSelector)
    {
        ArgumentNullException.ThrowIfNull(viewSelector, nameof(viewSelector));

        this.viewSelector = viewSelector;
        this.projects.AddRange(this.viewSelector.Projects.Select(model => new ProjectItemViewModel(model, this.viewSelector)));
        this.Projects = new(this.projects);

        // Synchronize the view model collection with the model collection
        ((INotifyCollectionChanged)this.viewSelector.Projects).CollectionChanged += this.OnProjectsChanged;

        // Visualize the active project view
        this.viewSelector.ViewChanged += (_, _) =>
        {
            this.projects.ForEach(p => p.IsSelected = p.Match(this.viewSelector.CurrentView as ProjectItem));
            this.OnPropertyChanged(nameof(this.IsHomeSelected));
            this.OnPropertyChanged(nameof(this.IsSettingsSelected));
        };
    }

    /// <summary>
    /// Gets or sets the project dependency.
    /// </summary>
    [Dependency]
    public IProject? Project { get; set; }

    /// <summary>
    /// Gets the projects.
    /// </summary>
    public ReadOnlyObservableCollection<ProjectItemViewModel> Projects { get; }

    /// <summary>
    /// Gets a value indicating whether the Home view is selected.
    /// </summary>
    public bool IsHomeSelected => this.viewSelector.CurrentView is ViewSelectorFeature.HomeItem;

    /// <summary>
    /// Gets a value indicating whether the Settings view is selected.
    /// </summary>
    public bool IsSettingsSelected => this.viewSelector.CurrentView is ViewSelectorFeature.SettingsItem;

    /// <summary>
    /// Activates the Home view.
    /// </summary>
    [RelayCommand]
    public void ActivateHome() => this.viewSelector.ActivateHome();

    /// <summary>
    /// Activates the Settings view.
    /// </summary>
    [RelayCommand]
    public void ActivateSettings() => this.viewSelector.ActivateSettings();

    /// <summary>
    /// Adds a new project item and activates its view.
    /// </summary>
    /// <returns>The awaitable <see cref="Task"/>.</returns>
    [RelayCommand]
    public async Task AddProject()
    {
        if (this.Project is null)
        {
            return;
        }

        var item = await this.Project.CreateProject();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            this.viewSelector.AddProject(item);
            this.viewSelector.ActivateProject(item);
        });
    }

    /// <summary>
    /// Prompts the user for folder to open and opens a project from it.
    /// </summary>
    [RelayCommand]
    public async void OpenProject()
    {
        if (App.Current?.MainWindow is { } parent)
        {
            var title = this.Host!.UI.GetLocalizedText("Open");

            var folders = await parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                SuggestedStartLocation = this.lastSelectedDirectory,
                AllowMultiple = false,
            });

            var folder = folders?.FirstOrDefault();
            if (folder is not null)
            {
                this.lastSelectedDirectory = folder;
                var path = folder.Path.LocalPath;

                // TODO: If the opened folder contains a recognized project file, examine and extract Title, Icon, and ProjectType info, then add to projects list
                // For now, just add a project item
                var item = new ProjectItem { Title = "Example", Path = path, ProjectType = ProjectType.ProjectType1, Icon = default };
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.viewSelector.AddProject(item);
                    this.viewSelector.ActivateProject(item);
                });
            }
        }
    }

    /// <inheritdoc />
    public override void Activate()
    {
        // When the view loads, activate the Home screen by default.
        this.viewSelector.ActivateHome();
    }

    /// <inheritdoc />
    protected override void Initialize()
    {
        if (this.Project is null)
        {
            Trace.TraceError($"{nameof(ViewSelectorViewModel)}: Required dependency {nameof(IProject)} could not be resolved. Feature may not work properly.");
        }
    }

    /// <summary>
    /// Called when the <see cref="IViewSelector.Projects"/> collection changes, and synchronizes the <see cref="Projects"/> accordingly.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    /// <exception cref="System.NotImplementedException">The Replace and Move actions are unsupported.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">The Action was unrecognized, and thus unsupported.</exception>
    private void OnProjectsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                e.NewItems?.OfType<ProjectItem>().Reverse().ForEach(i => this.projects.Insert(e.NewStartingIndex, new(i, this.viewSelector)));
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var model in e.OldItems?.OfType<ProjectItem>() ?? Enumerable.Empty<ProjectItem>())
                {
                    var viewmodel = this.projects.FirstOrDefault(vm => vm.Match(model));
                    if (viewmodel is not null)
                    {
                        this.projects.Remove(viewmodel);
                    }
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Move:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Reset:
                this.projects.Clear();
                this.projects.AddRange(this.viewSelector.Projects.Select(model => new ProjectItemViewModel(model, this.viewSelector)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e));
        }
    }
}