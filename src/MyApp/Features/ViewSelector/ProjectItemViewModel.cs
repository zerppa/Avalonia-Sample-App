namespace MyApp.Features.ViewSelector;

using System;
using System.ComponentModel;

using Avalonia.Utilities;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// The view model for a single project item.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class ProjectItemViewModel : ViewModel
{
    private readonly ProjectItem model;
    private readonly IViewSelector viewSelector;

    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectItemViewModel"/> class.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="viewSelector">The view selector.</param>
    /// <exception cref="System.ArgumentNullException">
    /// item
    /// or
    /// viewSelector
    /// </exception>
    public ProjectItemViewModel(ProjectItem item, IViewSelector viewSelector)
    {
        this.model = item ?? throw new ArgumentNullException(nameof(item));
        this.viewSelector = viewSelector ?? throw new ArgumentNullException(nameof(viewSelector));

        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, ProjectItemViewModel>(
            item,
            nameof(INotifyPropertyChanged.PropertyChanged),
            this.OnProjectItemPropertyChanged);
    }

    /// <summary>
    /// Gets the project title.
    /// </summary>
    public string Title => this.model.Title;

    /// <summary>
    /// Gets the project type.
    /// </summary>
    public ProjectType ProjectType => this.model.ProjectType;

    /// <summary>
    /// Gets the icon.
    /// </summary>
    public object? Icon => this.model.Icon;

    /// <summary>
    /// Gets a value indicating whether the item has unsaved changes.
    /// </summary>
    public bool IsDirty => this.model.IsDirty;

    /// <summary>
    /// Activates the view corresponding this view model.
    /// </summary>
    [RelayCommand]
    public new void Activate()
    {
        this.viewSelector.ActivateProject(this.model);
    }

    /// <summary>
    /// Attempts to remove this item from the list.
    /// </summary>
    [RelayCommand]
    public void Remove()
    {
        this.viewSelector.RemoveProject(this.model);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> matches the underlying model instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <remarks>Instead of exposing the model instance, we provide a test for equals.</remarks>
    /// <returns><c>true</c> if the test succeeds.</returns>
    internal bool Match(ProjectItem? item) => this.model == item;

    /// <summary>
    /// Called when the <see cref="INotifyPropertyChanged.PropertyChanged"/> fires on the associated <see cref="ProjectItem"/>.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void OnProjectItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectItem.Title))
        {
            this.OnPropertyChanged(nameof(this.Title));
        }

        if (e.PropertyName == nameof(ProjectItem.Icon))
        {
            this.OnPropertyChanged(nameof(this.Icon));
        }

        if (e.PropertyName == nameof(ProjectItem.IsDirty))
        {
            this.OnPropertyChanged(nameof(this.IsDirty));
        }
    }
}