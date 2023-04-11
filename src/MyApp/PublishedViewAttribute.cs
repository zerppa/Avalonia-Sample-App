namespace MyApp;

using System;

using Avalonia.Controls;

/// <summary>
/// Declares the the specified type as a published view.
/// </summary>
/// <typeparam name="TView">The type of the view. Must be derived from <see cref="UserControl"/></typeparam>
/// <seealso cref="MyApp.IPublishedViewInfo" />
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class PublishedViewAttribute<TView> : Attribute, IPublishedViewInfo
    where TView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedViewAttribute{TView}"/> class.
    /// </summary>
    /// <param name="viewName">The view name.</param>
    public PublishedViewAttribute(string viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName, nameof(viewName));

        this.ViewName = viewName;
    }

    /// <summary>
    /// Gets the view name.
    /// </summary>
    public string ViewName { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the view type.
    /// </summary>
    public Type ViewType => typeof(TView);
}