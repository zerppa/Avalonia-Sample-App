namespace MyApp;

using System;

/// <summary>
/// Describes a Published View.
/// </summary>
public interface IPublishedViewInfo
{
    /// <summary>
    /// Gets the view name.
    /// </summary>
    string ViewName { get; }

    /// <summary>
    /// Gets the view type.
    /// </summary>
    Type ViewType { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    string? Description { get; }
}