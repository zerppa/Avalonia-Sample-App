namespace MyApp.Features.ViewSelector;

using System;
using System.Collections.ObjectModel;

/// <summary>
/// Controls the View Selector.
/// </summary>
public interface IViewSelector
{
    /// <summary>
    /// Occurs when the active view has changed.
    /// </summary>
    event EventHandler<ViewChangedEventArgs> ViewChanged;

    /// <summary>
    /// Gets the projects.
    /// </summary>
    ReadOnlyObservableCollection<ProjectItem> Projects { get; }

    /// <summary>
    /// Gets the current view.
    /// </summary>
    IViewItem CurrentView { get; }

    /// <summary>
    /// Activates the Home view.
    /// </summary>
    void ActivateHome();

    /// <summary>
    /// Activates the specified project view.
    /// </summary>
    /// <param name="item">The item.</param>
    void ActivateProject(ProjectItem item);

    /// <summary>
    /// Activates the Settings view.
    /// </summary>
    void ActivateSettings();

    /// <summary>
    /// Adds a new project item to the <see cref="Projects"/> list.
    /// </summary>
    /// <param name="item">The item.</param>
    void AddProject(ProjectItem item);

    /// <summary>
    /// Removes the specified project from the <see cref="Projects"/> list.
    /// </summary>
    /// <param name="item">The item.</param>
    void RemoveProject(ProjectItem item);
}