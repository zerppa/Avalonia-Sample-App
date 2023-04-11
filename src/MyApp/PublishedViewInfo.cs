namespace MyApp;

using System;
using System.Diagnostics;

using Avalonia.Controls;

/// <summary>
/// Published view information.
/// </summary>
/// <seealso cref="MyApp.IPublishedViewInfo" />
public sealed class PublishedViewInfo : IPublishedViewInfo
{
    private readonly Func<object?, ViewModel?> factoryMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedViewInfo" /> class.
    /// </summary>
    /// <param name="source">The source feature.</param>
    /// <param name="factoryMethod">The view model factory method.</param>
    /// <param name="info">The published view information.</param>
    public PublishedViewInfo(App.Feature source, Func<object?, ViewModel?> factoryMethod, IPublishedViewInfo info)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(factoryMethod, nameof(factoryMethod));
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        this.Source = source;
        this.factoryMethod = factoryMethod;
        this.ViewName = info.ViewName;
        this.ViewType = info.ViewType;
        this.Description = info.Description;
    }

    /// <inheritdoc />
    public string ViewName { get; }

    /// <inheritdoc />
    public Type ViewType { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <summary>
    /// Gets the source feature.
    /// </summary>
    private App.Feature Source { get; }

    /// <summary>
    /// Creates an instance of the view.
    /// </summary>
    /// <param name="parameter">The view parameter.</param>
    /// <returns>An instance of the view or null if the view could not be created.</returns>
    internal UserControl? CreateView(object? parameter)
    {
        return this.CreateView(parameter, (view, viewModel) => view);
    }

    /// <summary>
    /// Creates the view.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="parameter">The view parameter.</param>
    /// <param name="result">The delegate that converts the result to the desired type.</param>
    /// <returns>The view or null if the view could not be created.</returns>
    private TResult? CreateView<TResult>(object? parameter, Func<UserControl, ViewModel, TResult> result)
        where TResult : UserControl
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        try
        {
            var viewModel = this.factoryMethod(parameter);

            if (viewModel == null)
            {
                return null;
            }

            var view = result((UserControl)Activator.CreateInstance(this.ViewType)!, viewModel);

            App.Current?.ConfigureView(this.Source, this.ViewName, view, viewModel);

            return view;
        }
        catch (Exception e)
        {
            Trace.TraceError($"Could not create Published View '{this.ViewName}'. {e.Message}");
        }

        return null;
    }
}