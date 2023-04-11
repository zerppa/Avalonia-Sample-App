namespace MyApp.Features.Shell;

using Avalonia.Controls;

/// <summary>
/// Adds or removes views from the main workspace.
/// </summary>
internal interface IViewContainer
{
    /// <summary>
    /// Adds a control to the workspace (unless it already exists) and makes it visible. Other controls will be hidden.
    /// </summary>
    /// <param name="control">The control.</param>
    void AddOrActivate(Control? control);

    /// <summary>
    /// Removes the specified control from the workspace. Does not affect other controls' visibility.
    /// </summary>
    /// <param name="control">The control.</param>
    void Remove(Control? control);
}