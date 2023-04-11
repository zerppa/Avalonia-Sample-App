namespace MyApp.Features.ViewSelector;

using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Represents a single project item in the projects list.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
/// <seealso cref="MyApp.Features.ViewSelector.IViewItem" />
public partial class ProjectItem : ObservableObject, IViewItem
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string? path;

    [ObservableProperty]
    private object? icon;

    [ObservableProperty]
    private bool isDirty;

    /// <summary>
    /// Gets the project type.
    /// </summary>
    public required ProjectType ProjectType { get; init; }

    /// <summary>
    /// Gets or sets the Save delegate.
    /// </summary>
    public Func<Task>? SaveAsync { get; set; }
}