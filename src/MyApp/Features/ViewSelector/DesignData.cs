namespace MyApp.Features.ViewSelector;

using System;
using System.Collections.ObjectModel;

/// <summary>
/// Provides design time support.
/// </summary>
public static class DesignData
{
    /// <summary>
    /// Gets design time view model instance.
    /// </summary>
    public static ViewSelectorViewModel ViewModel { get; } = new(new DesignInterface());

    /// <summary>
    /// The mock class for the Designer.
    /// </summary>
    /// <seealso cref="MyApp.Features.ViewSelector.IViewSelector" />
    private class DesignInterface : IViewSelector
    {
        public event EventHandler<ViewChangedEventArgs>? ViewChanged;

        public ReadOnlyObservableCollection<ProjectItem> Projects { get; } = new(new());

        public IViewItem CurrentView { get; }

        public void ActivateHome()
        {
        }

        public void ActivateProject(ProjectItem item)
        {
        }

        public void ActivateSettings()
        {
        }

        public void AddProject(ProjectItem item)
        {
        }

        public void RemoveProject(ProjectItem item)
        {
        }
    }
}