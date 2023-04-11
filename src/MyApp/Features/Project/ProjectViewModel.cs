namespace MyApp.Features.Project;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Utilities;

using CommunityToolkit.Mvvm.Input;

using MyApp.Features.ViewSelector;

/// <summary>
/// The view model for project workspace.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class ProjectViewModel : ViewModel
{
    private readonly ProjectItem item;

    private string content = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectViewModel"/> class.
    /// </summary>
    /// <param name="item">The item.</param>
    public ProjectViewModel(ProjectItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        item.SaveAsync = this.Save;
        this.item = item;

        var seed = this.item.Path?.Select(c => (int)c).Aggregate((sum, i) => sum ^ i) ?? 0;
        Debug.WriteLine(seed);

        var random = new Random(seed);
        this.Id = $"{(char)random.Next('A', 'Z')}-{(char)random.Next('A', 'Z')}-{(char)random.Next('A', 'Z')}";

        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, ProjectViewModel>(
            this.item,
            nameof(INotifyPropertyChanged.PropertyChanged),
            this.OnProjectItemPropertyChanged);
    }

    /// <summary>
    /// Gets the identifier. This is a string to distinguish demo projects from each other.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the project title.
    /// </summary>
    public string ProjectTitle => this.item.Title;

    /// <summary>
    /// Gets a value indicating whether the project has unsaved changes.
    /// </summary>
    public bool IsDirty => this.item.IsDirty;

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    public string Content
    {
        get => this.content;
        set
        {
            this.content = value;
            this.item.IsDirty = true;
        }
    }

    /// <summary>
    /// Mimics saving functionality, resetting the <see cref="IsDirty" /> state.
    /// </summary>
    /// <returns>The awaitable <see cref="Task"/>.</returns>
    [RelayCommand]
    public async Task Save()
    {
        await Task.Yield();

        this.item.IsDirty = false;
    }

    /// <summary>
    /// Called when the <see cref="INotifyPropertyChanged.PropertyChanged"/> fires on the associated <see cref="ProjectItem"/>.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void OnProjectItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectItem.Title))
        {
            this.OnPropertyChanged(nameof(this.ProjectTitle));
        }

        if (e.PropertyName == nameof(ProjectItem.IsDirty))
        {
            this.OnPropertyChanged(nameof(this.IsDirty));
        }
    }
}